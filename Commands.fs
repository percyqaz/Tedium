namespace Tedium

open System

module Commands =

    let navigate_up (state: State) : unit =
        let index = state.List.Items.IndexOf(state.Selected)
        let new_index = (index + state.List.Items.Count - 1) % state.List.Items.Count
        state.Selected <- state.List.Items.[new_index]

    let navigate_down (state: State) : unit =
        let index = state.List.Items.IndexOf(state.Selected)
        let new_index = (index + 1) % state.List.Items.Count
        state.Selected <- state.List.Items.[new_index]

    let move_up (state: State) : unit =
        let index = state.List.Items.IndexOf(state.Selected)

        if index > 0 then
            state.List.Items.RemoveAt(index)
            state.List.Items.Insert(index - 1, state.Selected)

    let move_down (state: State) : unit =
        let index = state.List.Items.IndexOf(state.Selected)

        if index + 1 < state.List.Items.Count then
            state.List.Items.RemoveAt(index)
            state.List.Items.Insert(index + 1, state.Selected)

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
        | "mark_done" -> state.Selected.MarkDone()
        | "unmark_done" -> state.Selected.UnmarkDone()
        | "edit" -> Operations.edit(state.List, state.Selected)
        | "gdesc" -> Operations.edit_fm(state.List)
        | "desc" -> Operations.edit_fm(state.Selected)
        | "rename" -> Operations.edit_name(state.List, state.Selected)
        | _ -> ()

    let send_text (state: State, text: string) : unit =
        match Tag.TryParse(text) with
        | true, tag -> state.Selected.AddTag(tag)
        | false, _ ->

        if text <> "" then
            let data = TodoItemParser.ParseLines([ text ]).ToTodoFile("")
            let index = state.List.Items.IndexOf(state.Selected)
            state.List.Items.InsertRange(index + 1, data.Items)
            state.List.FrontMatter.AddRange(data.FrontMatter)

            if data.Items.Count > 0 then
                state.Selected <- data.Items.[data.Items.Count - 1]
