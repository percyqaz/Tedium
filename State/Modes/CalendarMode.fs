namespace Tedium

open System
open System.Globalization

[<Struct>]
type CalendarDay =
    {
        Date: DateOnly
        Items: TodoElement array
    }

    static member Create(date: DateOnly, root: TodoListRoot) : CalendarDay =
        let date_string = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)

        let inline matches_date (item: TodoElement) : bool =
            item.GetTagValue("date") = Some(ValueSome date_string)

        let results = ResizeArray<TodoElement>()

        let rec search (parent: TodoElement) : unit =
            for item in parent.Items do
                if matches_date(item) then
                    results.Add(item)

                search(item)

        search(root.RootElement)

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
        {
            Root = this.Root
            Stack = this.Stack
            Scope = this.Scope
            Selection =
                match this.Selected with
                | Some item -> Some(this.Scope.Items.IndexOf(item))
                | None -> None
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

    member this.Selected: TodoElement option =
        match this.Selection with
        | Some index -> Some(this.View.[0].Days.[int this.Day].Items.[index])
        | None -> None
