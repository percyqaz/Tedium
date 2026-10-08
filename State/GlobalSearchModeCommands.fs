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
    static member NavigateRight(sm: GlobalSearchMode) : unit = sm.Open()

    [<Extension>]
    static member NavigateLeft(sm: GlobalSearchMode) : unit = ignore(sm.Close())

    [<Extension>]
    static member Delete(sm: GlobalSearchMode) : unit =
        match sm.Selected with
        | Some item ->
            sm.NavigateUp()
            sm.FindParent(item) |> Option.iter(fun (parent, _) -> parent.Items.Remove(item) |> ignore)
            sm.Refresh()
        | None -> sm.Scope.FrontMatter.Clear()

    [<Extension>]
    static member Edit(sm: GlobalSearchMode) : unit =
        match sm.Selected with
        | Some item ->
            let parent = sm.FindParent(item) |> Option.map fst |> Option.defaultValue sm.Scope

            Operations.edit(parent, item)
            sm.Refresh()
        | None -> Operations.edit_frontmatter(sm.Scope)

    [<Extension>]
    static member Describe(sm: GlobalSearchMode) : unit =
        match sm.Selected with
        | Some item ->
            Operations.edit_contents(item)
            sm.Refresh()
        | None -> Operations.edit_frontmatter(sm.Scope)

    [<Extension>]
    static member Rename(sm: GlobalSearchMode) : unit =
        match sm.Selected with
        | Some item ->
            let parent = sm.FindParent(item) |> Option.map fst |> Option.defaultValue sm.Scope

            Operations.edit_name(parent, item)
            sm.Refresh()
        | None -> ()
