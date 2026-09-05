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

    [<Extension>]
    static member NavigateDown(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.NavigateDown()

    [<Extension>]
    static member NavigateRight(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.NavigateRight()

    [<Extension>]
    static member NavigateLeft(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.NavigateLeft()

    [<Extension>]
    static member Open(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm -> nm.Open()

    [<Extension>]
    static member Close(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm ->
            if not(nm.Close()) then
                state.Running <- false

    [<Extension>]
    static member MoveUp(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.MoveUp()

    [<Extension>]
    static member MoveDown(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.MoveDown()

    [<Extension>]
    static member MoveRight(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.MoveRight()

    [<Extension>]
    static member MoveLeft(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.MoveLeft()

    [<Extension>]
    static member Delete(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.Delete()

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

    [<Extension>]
    static member Describe(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.Describe()

    [<Extension>]
    static member Rename(state: State) : unit =
        state.MarkDirty()

        match state.Mode with
        | Mode.Normal nm -> nm.Rename()

    [<Extension>]
    static member ShowGitHubIssue(state: State) : unit =
        match state.Mode with
        | Mode.Normal nm ->
            match nm.Selected with
            | None ->
                match nm.Scope.GetTagValue("repo") with
                | Some(ValueSome _) -> state.StatusLine <- "NYI"
                | _ -> state.StatusLine <- "No repo provided"
            | Some item ->
                match nm.Scope.GetTagValue("repo"), item.GetTagValue("gh") with
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
            state.MarkDirty()
            let data = TodoItemParser.ParseLines([ text ]).ToTodoFile("")

            match data.Items |> Seq.tryExactlyOne with
            | Some new_item ->
                match state.Mode with
                | Mode.Normal nm ->
                    state.MarkDirty()
                    nm.InsertNewItem(new_item)
            | None -> ()

        if text.StartsWith(':') then state.DispatchCommand(text.Substring(1))
        elif text.StartsWith('*') then parse_and_add_item()
        elif text.StartsWith('@') then parse_and_toggle_tag()
        else state.StatusLine <- sprintf "Unrecognised input: %s" text
