namespace Tedium

open System.Runtime.CompilerServices

type SearchModeCommands =

    [<Extension>]
    static member NavigateUp(sm: SearchMode) : unit =
        let item_count = sm.Results.Length

        match sm.Selection with
        | Some index -> sm.Selection <- if index = 0 then None else Some(index - 1)
        | None -> sm.Selection <- if item_count > 0 then Some(item_count - 1) else None

    [<Extension>]
    static member NavigateDown(sm: SearchMode) : unit =
        let item_count = sm.Results.Length

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
                let item = sm.Results.[index]
                let target = sm.Results.[index - 1]

                let inline make_tree_swap () : unit =
                    let target_origin = sm.Scope.Items.IndexOf(target)
                    sm.Scope.Items.Remove(item) |> ignore
                    sm.Scope.Items.Insert(target_origin, item)

                let inline make_array_swap () : unit =
                    let x = sm.Results.[index]
                    sm.Results.[index] <- sm.Results.[index - 1]
                    sm.Results.[index - 1] <- x
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
            if index + 1 < sm.Results.Length then
                let item = sm.Results.[index]
                let target = sm.Results.[index + 1]

                let inline make_tree_swap () : unit =
                    sm.Scope.Items.Remove(item) |> ignore
                    let target_origin = sm.Scope.Items.IndexOf(target)
                    sm.Scope.Items.Insert(target_origin + 1, item)

                let inline make_array_swap () : unit =
                    let x = sm.Results.[index]
                    sm.Results.[index] <- sm.Results.[index + 1]
                    sm.Results.[index + 1] <- x
                    sm.Selection <- Some(index + 1)

                let can_swap = sm.Query.SortKey(item) = sm.Query.SortKey(target)

                if can_swap then
                    make_tree_swap()
                    make_array_swap()
        | None -> ()

    [<Extension>]
    static member MoveRight(sm: SearchMode) : unit =
        match sm.Selection with
        | Some index when index > 0 ->
            let item = sm.Results.[index]
            let target_parent = sm.Results.[index - 1]
            target_parent.Items.Add(item)
            sm.Scope.Items.Remove(item) |> ignore
            sm.Refresh()
            sm.Selection <- Some(index - 1)
        | _ -> ()

    [<Extension>]
    static member MoveLeft(sm: SearchMode) : unit =
        match sm.Selection with
        | Some child_index ->
            match sm.Stack with
            | (parent, parent_index) :: _ ->
                let item = sm.Results.[child_index]
                parent.Items.Insert(parent_index + 1, item)
                sm.Scope.Items.Remove(item) |> ignore
                sm.Refresh()
                sm.Selection <- if child_index < sm.Results.Length then Some child_index else None
            | [] -> ()
        | None -> ()

    [<Extension>]
    static member Delete(sm: SearchMode) : unit =
        match sm.Selected with
        | Some item ->
            sm.NavigateUp()
            ignore(sm.Scope.Items.Remove(item))
            sm.Refresh()
        | None -> sm.Scope.FrontMatter.Clear()

    [<Extension>]
    static member Edit(sm: SearchMode) : unit =
        match sm.Selected with
        | Some item ->
            Operations.edit(sm.Scope, item)
            sm.Refresh()
        | None -> Operations.edit_frontmatter(sm.Scope)

    [<Extension>]
    static member Describe(sm: SearchMode) : unit =
        match sm.Selected with
        | Some item ->
            Operations.edit_contents(item)
            sm.Refresh()
        | None -> Operations.edit_frontmatter(sm.Scope)

    [<Extension>]
    static member Rename(sm: SearchMode) : unit =
        match sm.Selected with
        | Some item ->
            Operations.edit_name(sm.Scope, item)
            sm.Refresh()
        | None -> ()
