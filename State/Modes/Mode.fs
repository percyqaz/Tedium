namespace Tedium

[<RequireQualifiedAccess>]
type Mode =
    | Normal of NormalMode
    | Search of SearchMode
    | GlobalSearch of GlobalSearchMode
    | Calendar of CalendarMode

    member this.Root: TodoListRoot =
        match this with
        | Normal nm -> nm.Root
        | Search sm -> sm.Root
        | GlobalSearch sm -> sm.Root
        | Calendar cm -> cm.Root

    member this.Selected: TodoElement option =
        match this with
        | Normal nm -> nm.Selected
        | Search sm -> sm.Selected
        | GlobalSearch sm -> sm.Selected
        | Calendar cm -> cm.Selected

    member this.SearchBufferChanged(query: string) : Mode =
        let global_search = query.StartsWith('*')
        let search_text = if global_search then query.Substring(1) else query

        match this with
        | Normal nm ->
            if global_search then GlobalSearch(GlobalSearchMode.FromNormalMode(nm, search_text))
            elif query <> "" then Search(SearchMode.FromNormalMode(nm, query))
            else Normal nm
        | Search sm ->
            if global_search then GlobalSearch(GlobalSearchMode.FromNormalMode(sm.ToNormalMode(), search_text))
            elif query = "" then Normal(sm.ToNormalMode())
            else Search(SearchMode.FromNormalMode(sm.ToNormalMode(), query))
        | GlobalSearch sm ->
            if global_search then GlobalSearch(GlobalSearchMode.FromNormalMode(sm.ToNormalMode(), search_text))
            elif query <> "" then Search(SearchMode.FromNormalMode(sm.ToNormalMode(), query))
            else Normal(sm.ToNormalMode())
        | Calendar cm ->
            if global_search then GlobalSearch(GlobalSearchMode.FromNormalMode(cm.ToNormalMode(), search_text))
            elif query <> "" then Search(SearchMode.FromNormalMode(cm.ToNormalMode(), query))
            else Calendar cm
