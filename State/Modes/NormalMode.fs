namespace Tedium

type NormalMode =
    {
        Root: TodoListRoot
        mutable Stack: (TodoElement * int) list
        mutable Scope: TodoElement
        mutable Selection: int option
    }

    member this.Selected: TodoElement option =
        match this.Selection with
        | Some index -> Some(this.Scope.Items.[index])
        | None -> None

    member this.Open() : unit =
        match this.Selection with
        | Some index ->
            this.Stack <- (this.Scope, index) :: this.Stack
            this.Scope <- this.Scope.Items.[index]
            this.Selection <- None
        | None -> ()

    member this.Close() : bool =
        match this.Stack with
        | [] -> false
        | (previous, previous_selection) :: stack ->
            this.Stack <- stack
            this.Scope <- previous
            this.Selection <- Some previous_selection
            true

    member this.InsertNewItem(new_item: TodoElement) : unit =
        let index = this.Selection |> Option.defaultValue -1
        this.Scope.Items.Insert(index + 1, new_item)
        this.Selection <- Some(index + 1)

    static member Create(root: TodoListRoot) : NormalMode =
        {
            Root = root
            Stack = []
            Scope = root.RootElement
            Selection = None
        }
