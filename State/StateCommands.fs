namespace Tedium

open System
open System.Runtime.CompilerServices

type StateCommands =

    [<Extension>]
    static member Exit(state: State) : unit = state.Running <- false

    [<Extension>]
    static member NavigateUp(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.NavigateUp()
        | Mode.Search sm -> sm.NavigateUp()

    [<Extension>]
    static member NavigateDown(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.NavigateDown()
        | Mode.Search sm -> sm.NavigateDown()

    [<Extension>]
    static member NavigateRight(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.NavigateRight()
        | Mode.Search sm -> sm.NavigateRight()

    [<Extension>]
    static member NavigateLeft(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.NavigateLeft()
        | Mode.Search sm -> sm.NavigateLeft()

    [<Extension>]
    static member Open(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.Open()
        | Mode.Search sm -> sm.Open()

    [<Extension>]
    static member Close(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm ->
            if not(nm.Close()) then
                state.Running <- false
        | Mode.Search sm ->
            if not(sm.Close()) then
                state.Mode <- Mode.Normal(sm.ToNormalMode())

    [<Extension>]
    static member MoveUp(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.MoveUp()
        | Mode.Search sm -> sm.MoveUp()

    [<Extension>]
    static member MoveDown(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.MoveDown()
        | Mode.Search sm -> sm.MoveDown()

    [<Extension>]
    static member MoveRight(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.MoveRight()
        | Mode.Search sm -> state.StatusLine <- "NYI"

    [<Extension>]
    static member MoveLeft(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.MoveLeft()
        | Mode.Search sm -> state.StatusLine <- "NYI"

    [<Extension>]
    static member Delete(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.Delete()
        | Mode.Search sm -> state.StatusLine <- "NYI"

    [<Extension>]
    static member MarkDone(state: State) : unit =
        state.MarkDirty()
        state.Mode.Selected |> Option.iter _.MarkDone()

    [<Extension>]
    static member UnmarkDone(state: State) : unit =
        state.MarkDirty()
        state.Mode.Selected |> Option.iter _.UnmarkDone()

    [<Extension>]
    static member Edit(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.Edit()
        | Mode.Search sm -> state.StatusLine <- "NYI"

    [<Extension>]
    static member Describe(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.Describe()
        | Mode.Search sm -> state.StatusLine <- "NYI"

    [<Extension>]
    static member Rename(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.Rename()
        | Mode.Search sm -> state.StatusLine <- "NYI"

    [<Extension>]
    static member ShowGitHubIssue(state: State) : unit =
        let scope =
            match state.Mode with
            | Mode.Normal nm -> nm.Scope
            | Mode.Search sm -> sm.Scope

        match state.Mode.Selected with
        | None ->
            match scope.GetTagValue("repo") with
            | Some(ValueSome _) -> state.StatusLine <- "NYI"
            | _ -> state.StatusLine <- "No repo provided"
        | Some item ->
            match scope.GetTagValue("repo"), item.GetTagValue("gh") with
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
    static member Search(state: State) : unit =
        state.SearchBufferFocused <- not state.SearchBufferFocused

    [<Extension>]
    static member ColorTag(state: State, args: string) : unit =
        let split =
            args.Split('=', 2, StringSplitOptions.TrimEntries ||| StringSplitOptions.RemoveEmptyEntries)

        if split.Length < 2 then
            state.StatusLine <- "Requires 2 arguments separated by '='"
        else
            match Tag.TryParse(split.[0]) with
            | false, _ -> state.StatusLine <- "Invalid tag"
            | true, tag ->
                try
                    state.TagColors <- state.TagColors.Add(tag.Label, Convert.ToInt32(split.[1], 16))
                with err ->
                    state.StatusLine <- err.Message

    [<Extension>]
    static member DispatchCommand(state: State, command: string) : unit =
        let split = command.Split(" ", 2, StringSplitOptions.TrimEntries)
        let args = if split.Length < 2 then "" else split.[1]

        match split.[0] with
        | "q"
        | "q!"
        | "exit" -> state.Exit()
        | "up" -> state.NavigateUp()
        | "down" -> state.NavigateDown()
        | "left" -> state.NavigateLeft()
        | "right" -> state.NavigateRight()
        | "open" -> state.Open()
        | "close" -> state.Close()
        | "move_up" -> state.MoveUp()
        | "move_down" -> state.MoveDown()
        | "move_right" -> state.MoveRight()
        | "move_left" -> state.MoveLeft()
        | "mark_done" -> state.MarkDone()
        | "unmark_done" -> state.UnmarkDone()
        | "search" -> state.Search()
        | "edit" -> state.Edit()
        | "delete" -> state.Delete()
        | "describe" -> state.Describe()
        | "rename" -> state.Rename()
        | "color_tag" -> state.ColorTag(args)
        | "show_github_issue" -> state.ShowGitHubIssue()
        | _ -> state.StatusLine <- sprintf "Unrecognised command '%s'" split.[0]

    [<Extension>]
    static member DispatchMessage(state: State, text: string) : unit =

        let inline parse_and_toggle_tag () =
            state.MarkDirty()

            match Tag.TryParse(text) with
            | true, tag -> state.Mode.Selected |> Option.iter _.ToggleTag(tag)
            | false, _ -> ()

        let inline parse_and_add_item () : unit =
            let data = TodoItemParser.ParseLines([ text ]).ToTodoFile("")

            match data.Items |> Seq.tryExactlyOne with
            | Some new_item ->
                state.MarkDirty()

                match state.Mode with
                | Mode.Normal nm -> nm.InsertNewItem(new_item)
                | Mode.Search sm -> sm.InsertNewItem(new_item)
            | None -> ()

        if text.StartsWith(':') then state.DispatchCommand(text.Substring(1))
        elif text.StartsWith('*') then parse_and_add_item()
        elif text.StartsWith('@') then parse_and_toggle_tag()
        else state.StatusLine <- sprintf "Unrecognised input: %s" text
