namespace Tedium

open System.Runtime.CompilerServices

type CalendarModeCommands =

    [<Extension>]
    static member NavigateUp(cm: CalendarMode) : unit =
        match cm.Selection with
        | Some index ->
            let item_count = cm.SelectedDay.Items.Length
            cm.Selection <- if index = 0 then Some(item_count - 1) else Some(index - 1)
        | None ->
            cm.Week <- cm.Week.AddDays(-CalendarWeek.Length)
            cm.Refresh()

    [<Extension>]
    static member NavigateDown(cm: CalendarMode) : unit =

        match cm.Selection with
        | Some index ->
            let item_count = cm.SelectedDay.Items.Length
            cm.Selection <- if index + 1 >= item_count then Some(0) else Some(index + 1)
        | None ->
            cm.Week <- cm.Week.AddDays(CalendarWeek.Length)
            cm.Refresh()

    [<Extension>]
    static member NavigateRight(nm: CalendarMode) : unit = nm.Day <- enum((int nm.Day + 1) % 7)

    [<Extension>]
    static member NavigateLeft(nm: CalendarMode) : unit = nm.Day <- enum((int nm.Day + 6) % 7)
