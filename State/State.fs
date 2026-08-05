namespace Tedium

open System

type State =
    {
        mutable Running: bool
        mutable Dirty: bool
        Root: TodoListRoot
        mutable Stack: (TodoElement * TodoElement) list
        mutable Scope: TodoElement
        mutable Selected: TodoElement option
        CommandBuffer: CommandBuffer
        mutable StatusLine: string
    }

    member this.Open(element: TodoElement) : unit =
        this.Stack <- (this.Scope, element) :: this.Stack
        this.Scope <- element
        this.Selected <- None

    member this.Close() : unit =
        match this.Stack with
        | [] -> this.Running <- false
        | (previous, previous_selection) :: stack ->
            this.Stack <- stack
            this.Scope <- previous
            this.Selected <- Some previous_selection

    static member Create(path: string) : State =
        let root = TodoListRoot.Load(path)

        {
            Running = true
            Dirty = false
            Root = root
            Stack = []
            Scope = root.RootElement
            Selected = None
            CommandBuffer = CommandBuffer().SetDefaultBinds()
            StatusLine = ""
        }

    member this.MarkDirty() : unit = this.Dirty <- true

    member this.SaveChanges() : unit =
        if this.Dirty then
            this.Root.Save()
            this.StatusLine <- sprintf "Autosaved (%s)" (DateTime.Now.ToShortTimeString())
            this.Dirty <- false
