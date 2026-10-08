namespace Tedium

open System
open System.Globalization

type CalendarDayItem = { Item: TodoElement; Stack: (TodoElement * int) list; Scope: TodoElement }

[<Struct>]
type CalendarDay =
    {
        Date: DateOnly
        Items: CalendarDayItem array
    }

    static member Create(date: DateOnly, root: TodoListRoot) : CalendarDay =
        let date_string = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)

        let inline matches_date (item: TodoElement) : bool =
            item.GetTagValue("date") = Some(ValueSome date_string)

        let results = ResizeArray<CalendarDayItem>()

        let rec search (parent: TodoElement, stack: (TodoElement * int) list) : unit =
            for index = 0 to parent.Items.Count - 1 do
                let item = parent.Items.[index]

                if matches_date(item) then
                    results.Add({ Item = item; Scope = parent; Stack = stack })

                search(item, (parent, index) :: stack)

        search(root.RootElement, [])

        { Date = date; Items = results.ToArray() }

[<Struct>]
type CalendarWeek =
    {
        Days: CalendarDay array
    }

    static member val Length = 7
    static member val Count = 3

    static member Create(date: DateOnly, root: TodoListRoot) : CalendarWeek =
        if date.DayOfWeek <> DayOfWeek.Sunday then
            failwith "Weeks start on sunday"

        { Days = Array.init CalendarWeek.Length (fun i -> CalendarDay.Create(date.AddDays(i), root)) }

type CalendarMode =
    {
        Root: TodoListRoot
        mutable Stack: (TodoElement * int) list
        mutable Scope: TodoElement
        mutable Week: DateOnly
        mutable Day: DayOfWeek
        mutable Selection: int option
        mutable View: CalendarWeek array
    }

    member this.ToNormalMode() : NormalMode =
        match this.SelectedCalendarItem with
        | Some result ->
            {
                Root = this.Root
                Stack = result.Stack
                Scope = result.Scope
                Selection = Some(result.Scope.Items.IndexOf(result.Item))
            }
        | None ->
            {
                Root = this.Root
                Stack = this.Stack
                Scope = this.Scope
                Selection = None
            }

    static member FromNormalMode(nm: NormalMode) : CalendarMode =
        let today = DateOnly.FromDateTime(DateTime.Now)
        let start_of_week = today.AddDays(-(int today.DayOfWeek))

        {
            Root = nm.Root
            Stack = nm.Stack
            Scope = nm.Scope
            Week = start_of_week
            Day = today.DayOfWeek
            Selection = None
            View =
                Array.init
                    CalendarWeek.Count
                    (fun i -> CalendarWeek.Create(start_of_week.AddDays(CalendarWeek.Length * i), nm.Root))
        }

    member this.Refresh() : unit =
        this.View <-
            Array.init
                CalendarWeek.Count
                (fun i -> CalendarWeek.Create(this.Week.AddDays(CalendarWeek.Length * i), this.Root))

    member this.SelectedWeek: CalendarWeek = this.View.[0]

    member this.SelectedDay: CalendarDay = this.SelectedWeek.Days.[int this.Day]

    member this.Selected: TodoElement option =
        match this.Selection with
        | Some index -> Some(this.SelectedDay.Items.[index].Item)
        | None -> None

    member this.SelectedCalendarItem: CalendarDayItem option =
        match this.Selection with
        | Some index -> Some(this.SelectedDay.Items.[index])
        | None -> None

    member this.Open() : NormalMode option =
        match this.SelectedCalendarItem with
        | Some item ->
            Some
                {
                    Root = this.Root
                    Stack = item.Stack
                    Scope = item.Scope
                    Selection = Some(item.Scope.Items.IndexOf(item.Item))
                }
        | None -> None

    member this.InsertNewItem(new_item: TodoElement) : unit =
        let calendar_root =
            match this.Root.RootElement.Items |> Seq.tryFind(fun i -> i.GetTagValue("calendar") <> None) with
            | Some calendar -> calendar
            | None ->
                let new_calendar =
                    {
                        FrontMatter = ResizeArray()
                        Items = ResizeArray()
                        Guts =
                            Item { Text = "Calendar"; Done = false; Tags = Set.singleton(Tag.FromString("@calendar")) }
                    }

                this.Root.RootElement.Items.Add(new_calendar)
                new_calendar

        if new_item.GetTagValue("date").IsSome then
            new_item.ToggleTag(Tag.FromString("@date"))

        new_item.ToggleTag(
            Tag.FromString("@date:" + this.SelectedDay.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
        )

        calendar_root.Items.Add(new_item)
        this.Refresh()
