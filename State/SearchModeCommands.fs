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
