namespace Tedium

open System

module Tedium =

    let keymap: Keymap =
        let keymap = Keymap()
        let bind key command = keymap.AliasCommand(key, command)
        let alias key other_key = keymap.Alias(key, other_key)

        bind "h" "close"
        bind "j" "down"
        bind "k" "up"
        bind "l" "open"
        bind "x" "mark_done"
        bind "X" "unmark_done"
        bind "e" "edit"
        bind "r" "rename"
        bind "dd" "delete"
        bind "." "describe"
        bind (Keymap.SpecialKey("A-j")) "move_down"
        bind (Keymap.SpecialKey("A-k")) "move_up"
        bind (Keymap.SpecialKey("A-l")) "move_in"
        bind (Keymap.SpecialKey("A-h")) "move_out"
        bind "G" "show_github_issue"

        alias (Keymap.SpecialKey("Down")) "j"
        alias (Keymap.SpecialKey("Up")) "k"
        alias Keymap.ESC "h"
        alias Keymap.ENTER "l"

        keymap

    let loop (todo_file_path: string) : unit =
        let state = State.Create(todo_file_path)
        let input_thread = InputThread()

        let render = View(state)
        input_thread.Start()

        Console.Write(AnsiCodes.EnterSecondScreen)

        while state.Running do
            render.Redraw()

            match input_thread.TryReadKey(2000) with
            | true, input ->
                state.CommandBuffer.AddKey(input)
                state.CommandBuffer.Dispatch(state.DispatchMessage, keymap)
            | false, _ -> state.SaveChanges()

        Console.Write(AnsiCodes.LeaveSecondScreen)

        input_thread.Dispose()
        state.SaveChanges()
