namespace Tedium

type NormalMode =
    {
        Root: TodoListRoot
        mutable Stack: (TodoElement * int) list
        mutable Scope: TodoElement
        mutable Selection: int option
    }

    member this.Items = this.Scope.Items

    member this.Selected: TodoElement option =
        match this.Selection with
        | Some index -> Some(this.Items.[index])
        | None -> None

    member this.Open() : unit =
        match this.Selection with
        | Some index ->
            this.Stack <- (this.Scope, index) :: this.Stack
            this.Scope <- this.Items.[index]
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
        this.Items.Insert(index + 1, new_item)
        this.Selection <- Some(index + 1)

    static member Create(root: TodoListRoot) : NormalMode =
        {
            Root = root
            Stack = []
            Scope = root.RootElement
            Selection = None
        }
