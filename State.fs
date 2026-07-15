namespace Tedium

type State =
    {
        mutable Running: bool
        List: TodoElement
        mutable Selected: TodoElement
        mutable Buffer: string
    }

    static member Create(file: TodoElement) : State =
        {
            Running = true
            List = file
            Selected = file.Items.[0]
            Buffer = ""
        }
