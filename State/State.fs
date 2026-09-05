namespace Tedium

open System

type State =
    {
        mutable Running: bool
        mutable Dirty: bool
        mutable Mode: Mode
        CommandBuffer: CommandBuffer
        mutable TagColors: Map<string, int>
        mutable StatusLine: string
    }

    static member Create(path: string) : State =
        let root = TodoListRoot.Load(path)

        {
            Running = true
            Dirty = false
            Mode = Mode.Normal(NormalMode.Create(root))
            CommandBuffer = CommandBuffer()
            TagColors = Map.empty
            StatusLine = ""
        }

    member this.MarkDirty() : unit = this.Dirty <- true

    member this.SaveChanges() : unit =
        if this.Dirty then
            this.Mode.Root.Save()
            this.StatusLine <- sprintf "Autosaved (%s)" (DateTime.Now.ToShortTimeString())
            this.Dirty <- false
