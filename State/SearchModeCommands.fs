namespace Tedium

open System.Runtime.CompilerServices

type SearchModeCommands =

    [<Extension>]
    static member NavigateUp(sm: SearchMode) : unit =
        let item_count = sm.Items.Length

        match sm.Selection with
        | Some index -> sm.Selection <- if index = 0 then None else Some(index - 1)
        | None -> sm.Selection <- if item_count > 0 then Some(item_count - 1) else None

    [<Extension>]
    static member NavigateDown(sm: SearchMode) : unit =
        let item_count = sm.Items.Length

        match sm.Selection with
        | Some index -> sm.Selection <- if index + 1 >= item_count then None else Some(index + 1)
        | None -> sm.Selection <- if item_count > 0 then Some(0) else None

    [<Extension>]
    static member NavigateRight(sm: SearchMode) : unit = sm.Open()

    [<Extension>]
    static member NavigateLeft(sm: SearchMode) : unit = ignore(sm.Close())

    [<Extension>]
    static member MoveUp(sm: SearchMode) : unit =
        match sm.Selection with
        | Some index ->
            if index > 0 then
                let item = sm.Items.[index]
                let target = sm.Items.[index - 1]

                let inline make_tree_swap () : unit =
                    let target_origin = sm.Scope.Items.IndexOf(target)
                    sm.Scope.Items.Remove(item) |> ignore
                    sm.Scope.Items.Insert(target_origin, item)

                let inline make_array_swap () : unit =
                    let x = sm.Items.[index]
                    sm.Items.[index] <- sm.Items.[index - 1]
                    sm.Items.[index - 1] <- x
                    sm.Selection <- Some(index - 1)

                let can_swap = sm.Query.SortKey(item) = sm.Query.SortKey(target)

                if can_swap then
                    make_tree_swap()
                    make_array_swap()
        | None -> ()

    [<Extension>]
    static member MoveDown(sm: SearchMode) : unit =
        match sm.Selection with
        | Some index ->
            if index + 1 < sm.Items.Length then
                let item = sm.Items.[index]
                let target = sm.Items.[index + 1]

                let inline make_tree_swap () : unit =
                    let target_origin = sm.Scope.Items.IndexOf(target)
                    sm.Scope.Items.Remove(item) |> ignore
                    sm.Scope.Items.Insert(target_origin + 1, item)

                let inline make_array_swap () : unit =
                    let x = sm.Items.[index]
                    sm.Items.[index] <- sm.Items.[index + 1]
                    sm.Items.[index + 1] <- x
                    sm.Selection <- Some(index + 1)

                let can_swap = sm.Query.SortKey(item) = sm.Query.SortKey(target)

                if can_swap then
                    make_tree_swap()
                    make_array_swap()
        | None -> ()
