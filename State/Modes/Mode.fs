namespace Tedium

[<RequireQualifiedAccess>]
type Mode =
    | Normal of NormalMode
    | Search of SearchMode
    // GlobalSearch

    member this.Root: TodoListRoot =
        match this with
        | Normal nm -> nm.Root
        | Search sm -> sm.Root

    member this.Selected: TodoElement option =
        match this with
        | Normal nm -> nm.Selected
        | Search sm -> sm.Selected

    member this.Selection: int option =
        match this with
        | Normal nm -> nm.Selection
        | Search sm -> sm.Selection

    member this.SearchBufferChanged(query: string) : Mode =
        match this with
        | Normal nm -> if query <> "" then Search(SearchMode.FromNormalMode(nm, query)) else Normal nm
        | Search sm ->
            if query = "" then
                Normal(sm.ToNormalMode())
            else
                Search(SearchMode.FromNormalMode(sm.ToNormalMode(), query))
