namespace Tedium

open System

module Commands =

    let navigate_up (state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.Scope.Items.IndexOf(item)
            if index = 0 then state.Selected <- None else state.Selected <- Some state.Scope.Items.[index - 1]
        | None ->
            if state.Scope.Items.Count > 0 then
                state.Selected <- Some state.Scope.Items.[state.Scope.Items.Count - 1]

    let navigate_down (state: State) : unit =
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

    let navigate_in (state: State) : unit =
        match state.Selected with
        | Some item ->
            state.Open(item)
            state.Selected <- None
        | None -> ()

    let navigate_out (state: State) : unit =
        state.Close()
        state.Selected <- None

    let move_up (state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.Scope.Items.IndexOf(item)

            if index > 0 then
                state.Scope.Items.RemoveAt(index)
                state.Scope.Items.Insert(index - 1, item)
        | None -> ()

    let move_down (state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.Scope.Items.IndexOf(item)

            if index + 1 < state.Scope.Items.Count then
                state.Scope.Items.RemoveAt(index)
                state.Scope.Items.Insert(index + 1, item)
        | None -> ()

    let delete (state: State) : unit =
        match state.Selected with
        | Some item ->
            navigate_up(state)
            state.Scope.Items.Remove(item) |> ignore
        | None -> state.Scope.FrontMatter.Clear()

    let dispatch_internal_command (state: State, command: string) : unit =
        let split = command.Split(" ", 2, StringSplitOptions.TrimEntries)

        match split.[0] with
        | "q"
        | "q!"
        | "exit" -> state.Running <- false
        | "up" -> navigate_up(state)
        | "down" -> navigate_down(state)
        | "close" -> navigate_out(state)
        | "open" -> navigate_in(state)
        | "move_up" -> move_up(state)
        | "move_down" -> move_down(state)
        | "mark_done" -> state.Selected |> Option.iter _.MarkDone()
        | "unmark_done" -> state.Selected |> Option.iter _.UnmarkDone()
        | "edit" ->
            match state.Selected with
            | Some item -> Operations.edit(state.Scope, item)
            | None -> Operations.edit_fm(state.Scope)
        | "delete" -> delete(state)
        | "desc" ->
            match state.Selected with
            | Some item -> Operations.edit_contents(item)
            | None -> Operations.edit_fm(state.Scope)
        | "rename" ->
            match state.Selected with
            | Some item -> Operations.edit_name(state.Scope, item)
            | None -> ()
        | _ -> ()

    let send_text (state: State, text: string) : unit =
        match Tag.TryParse(text) with
        | true, tag -> state.Selected |> Option.iter _.ToggleTag(tag)
        | false, _ ->

        if text <> "" then
            let data = TodoItemParser.ParseLines([ text ]).ToTodoFile("")

            let index =
                match state.Selected with
                | Some item -> state.Scope.Items.IndexOf(item)
                | None -> -1

            state.Scope.Items.InsertRange(index + 1, data.Items)
            state.Scope.FrontMatter.AddRange(data.FrontMatter)

            if data.Items.Count > 0 then
                state.Selected <- Some data.Items.[data.Items.Count - 1]
