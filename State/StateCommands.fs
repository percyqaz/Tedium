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
        | Some item ->
            state.Open(item)
            state.Selected <- None
        | None -> ()

    [<Extension>]
    static member NavigateOut(state: State) : unit =
        state.Close()
        state.Selected <- None

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
    static member Delete(state: State) : unit =
        state.MarkDirty()

        match state.Selected with
        | Some item ->
            state.NavigateUp()
            state.Scope.Items.Remove(item) |> ignore
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
        | Some item -> Operations.edit(state.Scope, item)
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
        | "mark_done" -> state.MarkDone()
        | "unmark_done" -> state.UnmarkDone()
        | "edit" -> state.Edit()
        | "delete" -> state.Delete()
        | "desc" -> state.Describe()
        | "rename" -> state.Rename()
        | _ -> ()

    [<Extension>]
    static member DispatchText(state: State, text: string) : unit =
        let inline parse_and_toggle_tag () : bool =
            match Tag.TryParse(text) with
            | true, tag ->
                state.Selected |> Option.iter _.ToggleTag(tag)
                true
            | false, _ -> false

        let inline parse_and_add_item () : unit =
            let data = TodoItemParser.ParseLines([ text ]).ToTodoFile("")

            let index =
                match state.Selected with
                | Some item -> state.Scope.Items.IndexOf(item)
                | None -> -1

            state.Scope.Items.InsertRange(index + 1, data.Items)

            if data.Items.Count > 0 then
                state.Selected <- Some data.Items.[data.Items.Count - 1]

        if text.StartsWith(':') then
            state.DispatchCommand(text.Substring(1))
        elif parse_and_toggle_tag() then
            ()
        elif text.StartsWith('*') then
            parse_and_add_item()
