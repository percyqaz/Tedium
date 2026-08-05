namespace Tedium

type State =
    {
        mutable Running: bool
        Root: TodoListRoot
        mutable Stack: TodoElement list
        mutable Selected: TodoElement option
        CommandBuffer: CommandBuffer
        mutable Dirty: bool
    }

    member this.Scope: TodoElement = List.head this.Stack

    member this.Open(element: TodoElement) : unit = this.Stack <- element :: this.Stack

    member this.Close() : unit =
        let tail = List.tail this.Stack
        if tail = [] then this.Running <- false else this.Stack <- tail

    static member Create(path: string) : State =
        let root = TodoListRoot.Load(path)

        {
            Running = true
            Root = root
            Stack = [ root.RootElement ]
            Selected = None
            CommandBuffer = CommandBuffer().SetDefaultBinds()
            Dirty = false
        }

    member this.MarkDirty() : unit = this.Dirty <- true

    member this.SaveChanges() : unit =
        if this.Dirty then
            this.Root.Save()
            this.Dirty <- false
