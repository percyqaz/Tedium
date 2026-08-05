namespace Tedium

open System

type CommandDispatcher(state: State) =

    member this.DispatchCommand(command: string) : unit =
        let split = command.Split(" ", 2, StringSplitOptions.TrimEntries)

        match split.[0] with
        | "q"
        | "q!"
        | "exit" -> state.Exit()
        | "up" -> state.NavigateUp()
        | "down" -> state.NavigateDown()
        | "close" -> state.NavigateOut()
        | "open" -> state.NavigateIn()
        | "move_up" -> state.MoveUp()
        | "move_down" -> state.MoveDown()
        | "mark_done" -> state.MarkDone()
        | "unmark_done" -> state.UnmarkDone()
        | "edit" -> state.Edit()
        | "delete" -> state.Delete()
        | "desc" -> state.Describe()
        | "rename" -> state.Rename()
        | _ -> state.StatusLine <- sprintf "Unrecognised command '%s'" split.[0]

    member this.DispatchText(line: string) : unit =
        if line.StartsWith(':') then this.DispatchCommand(line.Substring(1)) else state.DispatchText(line)

    member this.DispatchCommandsOnState() : unit =
        state.CommandBuffer.DispatchCommands(this.DispatchText)

    member this.DispatchInitialCommandsOnState(config: string seq) : unit =
        state.CommandBuffer.DispatchInitialCommands(config, this.DispatchText)
