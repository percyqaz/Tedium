namespace Tedium

open System

module Commands =

    let navigate_up (state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.List.Items.IndexOf(item)
            if index = 0 then state.Selected <- None else state.Selected <- Some state.List.Items.[index - 1]
        | None ->
            if state.List.Items.Count > 0 then
                state.Selected <- Some state.List.Items.[state.List.Items.Count - 1]

    let navigate_down (state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.List.Items.IndexOf(item)

            if index + 1 >= state.List.Items.Count then
                state.Selected <- None
            else
                state.Selected <- Some state.List.Items.[index + 1]
        | None ->
            if state.List.Items.Count > 0 then
                state.Selected <- Some state.List.Items.[0]

    let move_up (state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.List.Items.IndexOf(item)

            if index > 0 then
                state.List.Items.RemoveAt(index)
                state.List.Items.Insert(index - 1, item)
        | None -> ()

    let move_down (state: State) : unit =
        match state.Selected with
        | Some item ->
            let index = state.List.Items.IndexOf(item)

            if index + 1 < state.List.Items.Count then
                state.List.Items.RemoveAt(index)
                state.List.Items.Insert(index + 1, item)
        | None -> ()

    let delete (state: State) : unit =
        match state.Selected with
        | Some item ->
            navigate_up(state)
            state.List.Items.Remove(item) |> ignore
        | None -> state.List.FrontMatter.Clear()


    let dispatch_internal_command (state: State, command: string) : unit =
        let split = command.Split(" ", 2, StringSplitOptions.TrimEntries)

        match split.[0] with
        | "q"
        | "q!"
        | "exit" -> state.Running <- false
        | "up" -> navigate_up(state)
        | "down" -> navigate_down(state)
        | "move_up" -> move_up(state)
        | "move_down" -> move_down(state)
        | "mark_done" -> state.Selected |> Option.iter _.MarkDone()
        | "unmark_done" -> state.Selected |> Option.iter _.UnmarkDone()
        | "edit" ->
            match state.Selected with
            | Some item -> Operations.edit(state.List, item)
            | None -> Operations.edit_fm(state.List)
        | "delete" -> delete(state)
        | "desc" ->
            match state.Selected with
            | Some item -> Operations.edit_contents(item)
            | None -> Operations.edit_fm(state.List)
        | "rename" ->
            match state.Selected with
            | Some item -> Operations.edit_name(state.List, item)
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
                | Some item -> state.List.Items.IndexOf(item)
                | None -> -1

            state.List.Items.InsertRange(index + 1, data.Items)
            state.List.FrontMatter.AddRange(data.FrontMatter)

            if data.Items.Count > 0 then
                state.Selected <- Some data.Items.[data.Items.Count - 1]
