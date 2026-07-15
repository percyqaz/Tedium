namespace Tedium

type State =
    {
        mutable Running: bool
        List: TodoFile
        mutable Selected: TodoItem
        mutable Buffer: string
    }

    static member Create(file: TodoFile) : State =
        {
            Running = true
            List = file
            Selected = file.Items.[0]
            Buffer = ""
        }
