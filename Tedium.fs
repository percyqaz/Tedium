namespace Tedium

open System

module Tedium =

    let loop (todo_file_path: string) : unit =
        let state = State.Create(todo_file_path)

        let render = View(state)
        let input_thread = InputThread()
        input_thread.Start()

        Console.Write("\u001b[?1049h")

        while state.Running do
            render.Redraw()

            match input_thread.TryReadKey(2000) with
            | true, input ->
                InputBuffer.add_input_to_buffer(input, state)
                InputBuffer.dispatch_keybindings(state)
            | false, _ -> state.SaveChanges()

        Console.Write("\u001b[?1049l")

        input_thread.Dispose()
        state.SaveChanges()
