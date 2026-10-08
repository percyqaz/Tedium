namespace Tedium

open System

type GlobalSearchResult = { Item: TodoElement; Stack: (TodoElement * int) list; Scope: TodoElement }

type GlobalSearchMode =
    {
        Root: TodoListRoot
        PreviousStack: (TodoElement * int) list
        PreviousScope: TodoElement
        mutable Selection: int option
        mutable Results: GlobalSearchResult array
        Query: SearchQuery
    }

    member this.ToNormalMode() : NormalMode =
        {
            Root = this.Root
            Stack = this.PreviousStack
            Scope = this.PreviousScope
            Selection =
                match this.Selected with
                | Some item ->
                    match this.PreviousScope.Items.IndexOf(item) with
                    | -1 -> None
                    | x -> Some x
                | None -> None
        }

    static member FromNormalMode(nm: NormalMode, query: string) : GlobalSearchMode =
        let parsed_query = SearchQuery.Parse(query)

        let mode =
            {
                Root = nm.Root
                PreviousStack = nm.Stack
                PreviousScope = nm.Scope
                Selection = None
                Query = parsed_query
                Results = [||]
            }

        mode.Refresh()

        mode.Selection <-
            let fallback = if mode.Results.Length = 1 then Some 0 else None

            match nm.Selected with
            | Some item ->
                Array.IndexOf(mode.Results, item)
                |> function
                    | -1 -> fallback
                    | x -> Some x
            | None -> fallback

        mode

    member this.Selected: TodoElement option =
        match this.Selection with
        | Some index -> Some(this.Results.[index].Item)
        | None -> None

    member this.SelectedSearchResult: GlobalSearchResult option =
        match this.Selection with
        | Some index -> Some(this.Results.[index])
        | None -> None

    member this.Refresh() : unit =
        let rec enumerate_all_items
            (parent: TodoElement, stack: (TodoElement * int) list)
            : seq<TodoElement * ((TodoElement * int) list * TodoElement)> =
            seq {
                for i = 0 to parent.Items.Count - 1 do
                    let item = parent.Items.[i]
                    yield item, (stack, parent)
                    yield! enumerate_all_items(item, ((parent, i) :: stack))
            }

        let items =
            this.Query.Apply(ResizeArray(enumerate_all_items(this.Root.RootElement, [])))
            |> Array.map(fun (item, (stack, scope)) -> { Item = item; Stack = stack; Scope = scope })

        this.Results <- items

    member this.Open() : NormalMode option =
        match this.SelectedSearchResult with
        | Some item ->
            Some
                {
                    Root = this.Root
                    Stack = item.Stack
                    Scope = item.Scope
                    Selection = Some(item.Scope.Items.IndexOf(item.Item))
                }
        | None -> None
