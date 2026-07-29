namespace Tedium

type State =
    {
        mutable Running: bool
        Root: TodoElement
        mutable Stack: TodoElement list
        mutable Selected: TodoElement option
        mutable Buffer: string
    }

    member this.Scope = List.head this.Stack

    member this.Open(element: TodoElement) : unit = this.Stack <- element :: this.Stack

    member this.Close() : unit =
        let tail = List.tail this.Stack
        if tail = [] then this.Running <- false else this.Stack <- tail

    static member Create(file: TodoElement) : State =
        {
            Running = true
            Root = file
            Stack = [ file ]
            Selected = None
            Buffer = ""
        }
