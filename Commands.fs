namespace Tedium

open System

module Commands =

    let mark_done (state: State) : unit = state.Selected.Done <- true

    let unmark_done (state: State) : unit = state.Selected.Done <- false

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
        | "exit" -> state.Running <- false
        | "up" -> navigate_up(state)
        | "down" -> navigate_down(state)
        | "mark_done" -> mark_done(state)
        | "unmark_done" -> unmark_done(state)
        | _ -> ()
