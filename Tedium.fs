namespace Tedium

open System

module Tedium =

    let loop (todo_file_path: string, config: string seq) : unit =
        let state = State.Create(todo_file_path)

        state.CommandBuffer.Append(config)
        state.CommandBuffer.Dispatch(state.DispatchMessage, state.Keymap)

        let render = View(state)

        Console.Write(AnsiCodes.EnterSecondScreen)
        let input_thread = InputThread()

        while state.Running do
            render.Redraw()

            match input_thread.TryReadKey(2000) with
            | true, input ->
                state.AddKey(input)
                state.CommandBuffer.Dispatch(state.DispatchMessage, state.Keymap)
            | false, _ -> state.SaveChanges()

        Console.Write(AnsiCodes.LeaveSecondScreen)

        state.SaveChanges()
