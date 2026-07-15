namespace Tedium

open System
open System.Threading

module Interactive =

    let loop (file: TodoFile) : unit =
        let state = State.Create(file)

        let render = View(state)
        let input_thread = InputThread()
        input_thread.Start()

        Console.Write("\u001b[?1049h")

        while state.Running do
            render.Redraw()

            match input_thread.TryReadKey(Timeout.Infinite) with
            | true, input ->
                InputBuffer.add_input_to_buffer(input, state)
                InputBuffer.dispatch_keybindings(state)
            | false, _ -> ()

        Console.Write("\u001b[?1049l")

        input_thread.Dispose()
