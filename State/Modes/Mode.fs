namespace Tedium

[<RequireQualifiedAccess>]
type Mode =
    | Normal of NormalMode
    | Search of SearchMode
    | Calendar of CalendarMode

    member this.Root: TodoListRoot =
        match this with
        | Normal nm -> nm.Root
        | Search sm -> sm.Root
        | Calendar cm -> cm.Root

    member this.Selected: TodoElement option =
        match this with
        | Normal nm -> nm.Selected
        | Search sm -> sm.Selected
        | Calendar cm -> cm.Selected

    member this.SearchBufferChanged(query: string) : Mode =
        match this with
        | Normal nm -> if query <> "" then Search(SearchMode.FromNormalMode(nm, query)) else Normal nm
        | Search sm ->
            if query = "" then
                Normal(sm.ToNormalMode())
            else
                Search(SearchMode.FromNormalMode(sm.ToNormalMode(), query))
        | Calendar cm ->
            if query <> "" then Search(SearchMode.FromNormalMode(cm.ToNormalMode(), query)) else Calendar cm
