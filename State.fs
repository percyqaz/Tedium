namespace Tedium

type State =
    {
        mutable Running: bool
        Root: TodoElement
        Scope: TodoElement
        mutable Selected: TodoElement option
        mutable Buffer: string
    }

    static member Create(file: TodoElement) : State =
        {
            Running = true
            Root = file
            Scope = file
            Selected = None
            Buffer = ""
        }
