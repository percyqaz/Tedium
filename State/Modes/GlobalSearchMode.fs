namespace Tedium

open System

type GlobalSearchMode =
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
                | Some item ->
                    match this.Scope.Items.IndexOf(item) with
                    | -1 -> None
                    | x -> Some x
                | None -> None
        }

    static member FromNormalMode(nm: NormalMode, query: string) : GlobalSearchMode =
        let parsed_query = SearchQuery.Parse(query)

        let rec all_items (parent: TodoElement) : seq<TodoElement> =
            seq {
                for item in parent.Items do
                    yield item
                    yield! all_items item
            }

        let items = parsed_query.Apply(ResizeArray(all_items nm.Root.RootElement))

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
        let rec all_items (parent: TodoElement) : seq<TodoElement> =
            seq {
                for item in parent.Items do
                    yield item
                    yield! all_items item
            }

        this.Results <- this.Query.Apply(ResizeArray(all_items this.Root.RootElement))

    // todo: scrap in favour of better data types
    member this.FindParent(item: TodoElement) : (TodoElement * int) option =
        let rec find (parent: TodoElement) : (TodoElement * int) option =
            parent.Items
            |> Seq.tryFindIndex(fun child -> obj.ReferenceEquals(child, item))
            |> function
                | Some index -> Some(parent, index)
                | None -> parent.Items |> Seq.tryPick find

        find this.Root.RootElement

    member this.Open() : unit =
        match this.Selected with
        | Some item ->
            let path =
                let rec find (parent: TodoElement) : (TodoElement * int) list option =
                    parent.Items
                    |> Seq.tryFindIndex(fun child -> obj.ReferenceEquals(child, item))
                    |> function
                        | Some index -> Some [ parent, index ]
                        | None ->
                            parent.Items
                            |> Seq.tryPick(fun child ->
                                find child |> Option.map(fun rest -> (parent, parent.Items.IndexOf(child)) :: rest)
                            )

                find this.Root.RootElement |> Option.defaultValue []

            this.Stack <- List.rev path
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

                match Array.IndexOf(this.Results, previous.Items.[previous_selection]) with
                | -1 -> fallback
                | x -> Some x

            true
