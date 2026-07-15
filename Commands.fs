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

    let dispatch_internal_command (state: State, command: string) : unit =
        let split = command.Split(" ", 2, StringSplitOptions.TrimEntries)

        match split.[0] with
        | "q"
        | "q!"
        | "exit" -> state.Running <- false
        | "up" -> navigate_up(state)
        | "down" -> navigate_down(state)
        | "mark_done" -> state.Selected.MarkDone()
        | "unmark_done" -> state.Selected.UnmarkDone()
        | "edit" -> Operations.edit(state.List, state.Selected)
        | "gdesc" -> Operations.edit_fm(state.List)
        | "desc" -> Operations.edit_fm(state.Selected)
        | "rename" -> Operations.edit_name(state.List, state.Selected)
        | _ -> ()
