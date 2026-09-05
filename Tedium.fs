namespace Tedium

open System

module Tedium =

    let keymap: Keymap =
        let keymap = Keymap()
        let bind key command = keymap.AliasCommand(key, command)
        let alias key other_key = keymap.Alias(key, other_key)

        bind "h" "left"
        bind "j" "down"
        bind "k" "up"
        bind "l" "right"
        bind Keymap.ESC "close"
        bind Keymap.ENTER "open"
        bind "x" "mark_done"
        bind "X" "unmark_done"
        bind "e" "edit"
        bind "r" "rename"
        bind "dd" "delete"
        bind "." "describe"
        bind (Keymap.SpecialKey("Tab")) "search"
        bind (Keymap.SpecialKey("A-h")) "move_left"
        bind (Keymap.SpecialKey("A-j")) "move_down"
        bind (Keymap.SpecialKey("A-k")) "move_up"
        bind (Keymap.SpecialKey("A-l")) "move_right"
        bind "G" "show_github_issue"

        alias (Keymap.SpecialKey("Down")) "j"
        alias (Keymap.SpecialKey("Up")) "k"

        keymap

    let loop (todo_file_path: string) : unit =
        let state = State.Create(todo_file_path)

        state.CommandBuffer.Append(
            [
                ":color_tag @date = ffff99"
                ":color_tag @gh = 446688"
                ":color_tag @repo = 776666"
                ":color_tag @wish = ffff44"
            ]
        )

        state.CommandBuffer.Dispatch(state.DispatchMessage, keymap)

        let render = View(state)

        Console.Write(AnsiCodes.EnterSecondScreen)
        let input_thread = InputThread()

        while state.Running do
            render.Redraw()

            match input_thread.TryReadKey(2000) with
            | true, input ->
                state.AddKey(input)
                state.CommandBuffer.Dispatch(state.DispatchMessage, keymap)
            | false, _ -> state.SaveChanges()

        Console.Write(AnsiCodes.LeaveSecondScreen)

        state.SaveChanges()
