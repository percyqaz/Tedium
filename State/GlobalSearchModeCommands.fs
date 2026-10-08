namespace Tedium

open System.Runtime.CompilerServices

type GlobalSearchModeCommands =

    [<Extension>]
    static member NavigateUp(sm: GlobalSearchMode) : unit =
        let item_count = sm.Results.Length

        match sm.Selection with
        | Some index -> sm.Selection <- if index = 0 then None else Some(index - 1)
        | None -> sm.Selection <- if item_count > 0 then Some(item_count - 1) else None

    [<Extension>]
    static member NavigateDown(sm: GlobalSearchMode) : unit =
        let item_count = sm.Results.Length

        match sm.Selection with
        | Some index -> sm.Selection <- if index + 1 >= item_count then None else Some(index + 1)
        | None -> sm.Selection <- if item_count > 0 then Some(0) else None

    [<Extension>]
    static member Delete(sm: GlobalSearchMode) : unit =
        match sm.SelectedSearchResult with
        | Some item ->
            sm.NavigateUp()
            ignore(item.Scope.Items.Remove(item.Item))
            sm.Refresh()
        | None -> ()

    [<Extension>]
    static member Edit(sm: GlobalSearchMode) : unit =
        match sm.SelectedSearchResult with
        | Some item ->
            Operations.edit(item.Scope, item.Item)
            sm.Refresh()
        | None -> ()

    [<Extension>]
    static member Describe(sm: GlobalSearchMode) : unit =
        match sm.Selected with
        | Some item ->
            Operations.edit_contents(item)
            sm.Refresh()
        | None -> ()

    [<Extension>]
    static member Rename(sm: GlobalSearchMode) : unit =
        match sm.SelectedSearchResult with
        | Some item ->
            Operations.edit_name(item.Scope, item.Item)
            sm.Refresh()
        | None -> ()
