namespace Tedium

open System.Linq
open System.Runtime.CompilerServices

type NormalModeCommands =

    [<Extension>]
    static member NavigateUp(nm: NormalMode) : unit =
        let item_count = nm.Scope.Items.Count

        match nm.Selection with
        | Some index -> nm.Selection <- if index = 0 then None else Some(index - 1)
        | None -> nm.Selection <- if item_count > 0 then Some(item_count - 1) else None

    [<Extension>]
    static member NavigateDown(nm: NormalMode) : unit =
        let item_count = nm.Scope.Items.Count

        match nm.Selection with
        | Some index -> nm.Selection <- if index + 1 >= item_count then None else Some(index + 1)
        | None -> nm.Selection <- if item_count > 0 then Some(0) else None

    [<Extension>]
    static member NavigateRight(nm: NormalMode) : unit = nm.Open()

    [<Extension>]
    static member NavigateLeft(nm: NormalMode) : unit = ignore(nm.Close())

    [<Extension>]
    static member MoveUp(nm: NormalMode) : unit =
        match nm.Selection with
        | Some index ->
            if index > 0 then
                let item = nm.Scope.Items.[index]
                nm.Scope.Items.RemoveAt(index)
                nm.Scope.Items.Insert(index - 1, item)
                nm.Selection <- Some(index - 1)
        | None -> ()

    [<Extension>]
    static member MoveDown(nm: NormalMode) : unit =
        match nm.Selection with
        | Some index ->
            if index + 1 < nm.Scope.Items.Count then
                let item = nm.Scope.Items.[index]
                nm.Scope.Items.RemoveAt(index)
                nm.Scope.Items.Insert(index + 1, item)
                nm.Selection <- Some(index + 1)
        | None -> ()

    [<Extension>]
    static member MoveRight(nm: NormalMode) : unit =
        match nm.Selection with
        | Some index when index > 0 ->
            let item = nm.Scope.Items.[index]
            let target_parent = nm.Scope.Items.[index - 1]
            target_parent.Items.Add(item)
            nm.Scope.Items.Remove(item) |> ignore
            nm.Selection <- Some(index - 1)
        | _ -> ()

    [<Extension>]
    static member MoveLeft(nm: NormalMode) : unit =
        match nm.Selection with
        | Some child_index ->
            match nm.Stack with
            | (parent, parent_index) :: _ ->
                let item = nm.Scope.Items.[child_index]
                nm.Scope.Items.Remove(item) |> ignore
                parent.Items.Insert(parent_index + 1, item)
                nm.Selection <- if child_index < nm.Scope.Items.Count then Some child_index else None
            | [] -> ()
        | None -> ()

    [<Extension>]
    static member Delete(nm: NormalMode) : unit =
        match nm.Selected with
        | Some item ->
            nm.NavigateUp()
            ignore(nm.Scope.Items.Remove(item))
        | None -> nm.Scope.FrontMatter.Clear()

    [<Extension>]
    static member Edit(nm: NormalMode) : unit =
        match nm.Selected with
        | Some item -> Operations.edit(nm.Scope, item)
        | None -> Operations.edit_frontmatter(nm.Scope)

    [<Extension>]
    static member Describe(nm: NormalMode) : unit =
        match nm.Selected with
        | Some item -> Operations.edit_contents(item)
        | None -> Operations.edit_frontmatter(nm.Scope)

    [<Extension>]
    static member Rename(nm: NormalMode) : unit =
        match nm.Selected with
        | Some item -> Operations.edit_name(nm.Scope, item)
        | None -> ()
