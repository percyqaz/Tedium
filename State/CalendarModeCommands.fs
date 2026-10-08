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
    static member NavigateRight(nm: CalendarMode) : unit =
        nm.Day <- enum((int nm.Day + 1) % 7)
        nm.Selection <- None

    [<Extension>]
    static member NavigateLeft(nm: CalendarMode) : unit =
        nm.Day <- enum((int nm.Day + 6) % 7)
        nm.Selection <- None

    [<Extension>]
    static member MoveUp(cm: CalendarMode) : unit =
        match cm.SelectedCalendarItem with
        | Some selected when cm.Selection.Value > 0 ->
            let target = cm.SelectedDay.Items.[cm.Selection.Value - 1]

            if selected.Scope = target.Scope then
                let selected_index = selected.Scope.Items.IndexOf(selected.Item)
                let target_index = selected.Scope.Items.IndexOf(target.Item)

                if selected_index >= 0 && target_index >= 0 then
                    selected.Scope.Items.RemoveAt(selected_index)
                    selected.Scope.Items.Insert(target_index, selected.Item)
                    cm.Refresh()

                    cm.Selection <-
                        cm.SelectedDay.Items
                        |> Array.tryFindIndex(fun entry -> obj.ReferenceEquals(entry.Item, selected.Item))
        | _ -> ()

    [<Extension>]
    static member MoveDown(cm: CalendarMode) : unit =
        match cm.SelectedCalendarItem with
        | Some selected when cm.Selection.Value + 1 < cm.SelectedDay.Items.Length ->
            let target = cm.SelectedDay.Items.[cm.Selection.Value + 1]

            if selected.Scope = target.Scope then
                let selected_index = selected.Scope.Items.IndexOf(selected.Item)
                let target_index = selected.Scope.Items.IndexOf(target.Item)

                if selected_index >= 0 && target_index >= 0 then
                    selected.Scope.Items.RemoveAt(selected_index)
                    let target_index_after_remove = selected.Scope.Items.IndexOf(target.Item)
                    selected.Scope.Items.Insert(target_index_after_remove + 1, selected.Item)
                    cm.Refresh()

                    cm.Selection <-
                        cm.SelectedDay.Items
                        |> Array.tryFindIndex(fun entry -> obj.ReferenceEquals(entry.Item, selected.Item))
        | _ -> ()

    [<Extension>]
    static member MoveLeft(cm: CalendarMode) : unit = cm.MoveSelectionByDays(-1)

    [<Extension>]
    static member MoveRight(cm: CalendarMode) : unit = cm.MoveSelectionByDays(1)

    [<Extension>]
    static member Delete(cm: CalendarMode) : unit =
        match cm.SelectedCalendarItem with
        | Some selected ->
            cm.NavigateUp()
            selected.Scope.Items.Remove(selected.Item) |> ignore
            cm.Refresh()

            if cm.SelectedDay.Items.Length = 0 then
                cm.Selection <- None
            elif cm.Selection |> Option.exists(fun index -> index >= cm.SelectedDay.Items.Length) then
                cm.Selection <- Some(cm.SelectedDay.Items.Length - 1)
        | None -> ()

    [<Extension>]
    static member Edit(cm: CalendarMode) : unit =
        match cm.SelectedCalendarItem with
        | Some selected ->
            Operations.edit(selected.Scope, selected.Item)
            cm.Refresh()
        | None -> ()

    [<Extension>]
    static member Describe(cm: CalendarMode) : unit =
        match cm.Selected with
        | Some item ->
            Operations.edit_contents(item)
            cm.Refresh()
        | None -> ()

    [<Extension>]
    static member Rename(cm: CalendarMode) : unit =
        match cm.SelectedCalendarItem with
        | Some selected ->
            Operations.edit_name(selected.Scope, selected.Item)
            cm.Refresh()
        | None -> ()
