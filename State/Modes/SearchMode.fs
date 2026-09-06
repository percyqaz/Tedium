namespace Tedium

open System

type SearchMode =
    {
        Root: TodoListRoot
        mutable Stack: (TodoElement * int) list
        mutable Scope: TodoElement
        mutable Selection: int option
        mutable Results: TodoElement array
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

    static member FromNormalMode(nm: NormalMode, query: string) : SearchMode =
        let parsed_query = SearchQuery.Parse(query)
        let items = parsed_query.Apply(nm.Scope.Items)

        {
            Root = nm.Root
            Stack = nm.Stack
            Scope = nm.Scope
            Selection =
                let fallback = if items.Length = 1 then Some 0 else None

                match nm.Selected with
                | Some item ->
                    Array.IndexOf(items, item)
                    |> function
                        | -1 -> fallback
                        | x -> Some x
                | None -> fallback
            Query = parsed_query
            Results = items
        }

    member this.Selected: TodoElement option =
        match this.Selection with
        | Some index -> Some(this.Results.[index])
        | None -> None

    member this.Refresh() : unit =
        this.Results <- this.Query.Apply(this.Scope.Items)

    member this.Open() : unit =
        match this.Selected with
        | Some item ->
            let original_index = this.Scope.Items.IndexOf(item)
            this.Stack <- (this.Scope, original_index) :: this.Stack
            this.Scope <- item
            this.Refresh()
            this.Selection <- None
        | None -> ()

    member this.Close() : bool =
        match this.Stack with
        | [] -> false
        | (previous, previous_selection) :: stack ->
            this.Stack <- stack
            this.Scope <- previous
            this.Refresh()

            this.Selection <-
                let fallback = if this.Results.Length = 1 then Some 0 else None

                match Array.IndexOf(this.Results, this.Scope.Items.[previous_selection]) with
                | -1 -> fallback
                | x -> Some x

            true

    member this.InsertNewItem(new_item: TodoElement) : unit =
        let index =
            match this.Selected with
            | Some item -> this.Scope.Items.IndexOf(item)
            | None -> -1

        this.Scope.Items.Insert(index + 1, new_item)
        this.Refresh()
        this.Selection <- Some((this.Selection |> Option.defaultValue -1) + 1)
