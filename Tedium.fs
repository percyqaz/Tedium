namespace Tedium

open System

module Tedium =

    let loop (todo_file_path: string) : unit =
        let state = State.Create(todo_file_path)
        let input_thread = InputThread()
        let command_dispatcher = CommandDispatcher(state)

        let render = View(state)
        input_thread.Start()

        Console.Write(AnsiCodes.EnterSecondScreen)

        while state.Running do
            render.Redraw()

            match input_thread.TryReadKey(2000) with
            | true, input ->
                state.CommandBuffer.AddKey(input)
                command_dispatcher.DispatchCommandsOnState()
            | false, _ -> state.SaveChanges()

        Console.Write(AnsiCodes.LeaveSecondScreen)

        input_thread.Dispose()
        state.SaveChanges()
