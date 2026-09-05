namespace Tedium

type SearchMode =
    {
        Root: TodoListRoot
        mutable Stack: (TodoElement * int) list
        mutable Scope: TodoElement
        mutable Selection: int option
        Items: TodoElement array
        Query: SearchQuery
    }

    member this.ToNormalMode() : NormalMode =
        {
            Root = this.Root
            Stack = this.Stack
            Scope = this.Scope
            Selection =
                match this.Selected with
                | Some item -> Some(this.Scope.Items.IndexOf(item))
                | None -> None
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

    static member FromNormalMode(nm: NormalMode, query: string) : SearchMode =
        let parsed_query = SearchQuery.Parse(query)

        {
            Root = nm.Root
            Stack = nm.Stack
            Scope = nm.Scope
            Selection = nm.Selection
            Query = parsed_query
            Items = parsed_query.Apply(nm.Scope.Items)
        }
