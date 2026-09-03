namespace Tedium

open System
open System.Runtime.CompilerServices

type StateCommands =

    [<Extension>]
    static member Exit(state: State) : unit = state.Running <- false

    [<Extension>]
    static member NavigateUp(state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.Scope.Items.IndexOf(item)
            if index = 0 then state.Selected <- None else state.Selected <- Some state.Scope.Items.[index - 1]
        | None ->
            if state.Scope.Items.Count > 0 then
                state.Selected <- Some state.Scope.Items.[state.Scope.Items.Count - 1]

    [<Extension>]
    static member NavigateDown(state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.Scope.Items.IndexOf(item)

            if index + 1 >= state.Scope.Items.Count then
                state.Selected <- None
            else
                state.Selected <- Some state.Scope.Items.[index + 1]
        | None ->
            if state.Scope.Items.Count > 0 then
                state.Selected <- Some state.Scope.Items.[0]

    [<Extension>]
    static member NavigateIn(state: State) : unit =
        match state.Selected with
        | Some item -> state.Open(item)
        | None -> ()

    [<Extension>]
    static member NavigateOut(state: State) : unit = state.Close()

    [<Extension>]
    static member MoveUp(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item ->
            let index = state.Scope.Items.IndexOf(item)

            if index > 0 then
                state.Scope.Items.RemoveAt(index)
                state.Scope.Items.Insert(index - 1, item)
        | None -> ()

    [<Extension>]
    static member MoveDown(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item ->
            let index = state.Scope.Items.IndexOf(item)

            if index + 1 < state.Scope.Items.Count then
                state.Scope.Items.RemoveAt(index)
                state.Scope.Items.Insert(index + 1, item)
        | None -> ()

    [<Extension>]
    static member MoveIn(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item ->
            let index = state.Scope.Items.IndexOf(item)

            if index > 0 then
                let target = state.Scope.Items.[index - 1]
                target.Items.Add(item)
                state.Scope.Items.Remove(item) |> ignore
                state.Selected <- Some target
        | None -> ()

    [<Extension>]
    static member MoveOut(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item ->
            match state.Stack with
            | (parent, container) :: _ ->
                let child_index = state.Scope.Items.IndexOf(item)
                let index = parent.Items.IndexOf(container)

                if container.Items.Remove(item) then
                    parent.Items.Insert(index + 1, item)

                    state.Selected <-
                        if child_index < state.Scope.Items.Count then Some state.Scope.Items.[child_index] else None
            | [] -> ()
        | None -> ()

    [<Extension>]
    static member Delete(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item ->
            state.NavigateUp()

            if state.Scope.Items.Remove(item) then
                state.StatusLine <- sprintf "Deleted %O" item
        | None -> state.Scope.FrontMatter.Clear()

    [<Extension>]
    static member MarkDone(state: State) : unit =
        state.MarkDirty()
        state.Selected |> Option.iter _.MarkDone()

    [<Extension>]
    static member UnmarkDone(state: State) : unit =
        state.MarkDirty()
        state.Selected |> Option.iter _.UnmarkDone()

    [<Extension>]
    static member Edit(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item ->
            state.NavigateUp()
            Operations.edit(state.Scope, item)
            state.NavigateDown()
        | None -> Operations.edit_fm(state.Scope)

    [<Extension>]
    static member Describe(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item -> Operations.edit_contents(item)
        | None -> Operations.edit_fm(state.Scope)

    [<Extension>]
    static member Rename(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item -> Operations.edit_name(state.Scope, item)
        | None -> ()

    [<Extension>]
    static member ShowGitHubIssue(state: State) : unit =
        match state.Selected with
        | None ->
            match state.Scope.GetTagValue("repo") with
            | Some(ValueSome _) -> state.StatusLine <- "NYI"
            | _ -> state.StatusLine <- "No repo provided"
        | Some item ->
            match state.Scope.GetTagValue("repo"), item.GetTagValue("gh") with
            | Some(ValueSome(repo)), Some(ValueSome(issue)) ->
                match GitHub.get_issue(repo, issue) with
                | Ok issue ->
                    Console.Clear()
                    issue.Print()
                    Console.ReadKey(true) |> ignore
                | Error reason -> state.StatusLine <- reason
            | _, Some _ -> state.StatusLine <- "No repo provided"
            | _ -> state.StatusLine <- "No repo/issue provided"

    [<Extension>]
    static member DispatchCommand(state: State, command: string) : unit =
        let split = command.Split(" ", 2, StringSplitOptions.TrimEntries)

        match split.[0] with
        | "q"
        | "q!"
        | "exit" -> state.Exit()
        | "up" -> state.NavigateUp()
        | "down" -> state.NavigateDown()
        | "close" -> state.NavigateOut()
        | "open" -> state.NavigateIn()
        | "move_up" -> state.MoveUp()
        | "move_down" -> state.MoveDown()
        | "move_in" -> state.MoveIn()
        | "move_out" -> state.MoveOut()
        | "mark_done" -> state.MarkDone()
        | "unmark_done" -> state.UnmarkDone()
        | "edit" -> state.Edit()
        | "delete" -> state.Delete()
        | "describe" -> state.Describe()
        | "rename" -> state.Rename()
        | "show_github_issue" -> state.ShowGitHubIssue()
        | _ -> state.StatusLine <- sprintf "Unrecognised command '%s'" split.[0]

    [<Extension>]
    static member DispatchMessage(state: State, text: string) : unit =
        state.MarkDirty()

        let inline parse_and_toggle_tag () =
            match Tag.TryParse(text) with
            | true, tag -> state.Selected |> Option.iter _.ToggleTag(tag)
            | false, _ -> ()

        let inline parse_and_add_item () : unit =
            let data = TodoItemParser.ParseLines([ text ]).ToTodoFile("")

            let index =
                match state.Selected with
                | Some item -> state.Scope.Items.IndexOf(item)
                | None -> -1

            state.Scope.Items.InsertRange(index + 1, data.Items)

            if data.Items.Count > 0 then
                state.Selected <- Some data.Items.[data.Items.Count - 1]

        if text.StartsWith(':') then state.DispatchCommand(text.Substring(1))
        elif text.StartsWith('*') then parse_and_add_item()
        elif text.StartsWith('@') then parse_and_toggle_tag()
        else state.StatusLine <- sprintf "Unrecognised input: %s" text
