namespace Tedium

open System

type State =
    {
        mutable Running: bool
        mutable Dirty: bool
        mutable Mode: Mode
        CommandBuffer: CommandBuffer
        SearchBuffer: TextBuffer
        mutable SearchBufferFocused: bool
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
            SearchBuffer = TextBuffer()
            SearchBufferFocused = false
            TagColors = Map.empty
            StatusLine = ""
        }

    member this.MarkDirty() : unit = this.Dirty <- true

    member this.SaveChanges() : unit =
        if this.Dirty then
            this.Mode.Root.Save()
            this.StatusLine <- sprintf "Autosaved (%s)" (DateTime.Now.ToShortTimeString())
            this.Dirty <- false

    member this.AddKey(input: ConsoleKeyInfo) : unit =
        if this.SearchBufferFocused then
            if this.SearchBuffer.TryAddKey(input) then
                this.Mode <- this.Mode.SearchBufferChanged(this.SearchBuffer.ToString())
            else
                this.SearchBufferFocused <- false
        else
            this.CommandBuffer.AddKey(input)
