namespace Tedium

open System
open System.Globalization

[<Struct>]
type Tag =
    {
        Value: string voption
        Label: string
    }

    override this.ToString() : string =
        match this.Value with
        | ValueSome v -> sprintf "@%s:%s" this.Label v
        | ValueNone -> sprintf "@%s" this.Label

    static member ParseSpecial(name: string) : Tag voption =

        let date_of_value (value: string) : Tag voption =
            ValueSome { Label = "date"; Value = ValueSome(value) }

        let day_of_week (day: DayOfWeek) : Tag voption =
            let mutable today = DateOnly.FromDateTime(DateTime.Today).AddDays(1)

            while today.DayOfWeek <> day do
                today <- today.AddDays(1)

            date_of_value(today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))

        match name with
        | "TODAY" -> date_of_value(DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
        | "TOMORROW" -> date_of_value(DateTime.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
        | "SUNDAY" -> day_of_week(DayOfWeek.Sunday)
        | "MONDAY" -> day_of_week(DayOfWeek.Monday)
        | "TUESDAY" -> day_of_week(DayOfWeek.Tuesday)
        | "WEDNESDAY" -> day_of_week(DayOfWeek.Wednesday)
        | "THURSDAY" -> day_of_week(DayOfWeek.Thursday)
        | "FRIDAY" -> day_of_week(DayOfWeek.Friday)
        | "SATURDAY" -> day_of_week(DayOfWeek.Saturday)
        | _ -> ValueNone

    static member TryParse(value: string, out: outref<Tag>) : bool =
        let inline acceptable_tag_char (c: char) : bool =
            Char.IsAsciiDigit(c) || Char.IsAsciiLetterLower(c) || c = '_' || c = '-'

        let inline acceptable_data_char (c: char) : bool =
            Char.IsAsciiLetterOrDigit(c) || c = '_' || c = '-' || c = ':' || c = '/'

        let TAG_SYMBOL = '@'
        let TAG_DATA_SEPARATOR = ':'

        if value.StartsWith(TAG_SYMBOL) && value.Length > 1 then
            let split = value.Substring(1).Split(TAG_DATA_SEPARATOR, 2)

            if not(String.forall acceptable_tag_char split.[0]) then
                match Tag.ParseSpecial(split.[0]) with
                | ValueSome special ->
                    out <- special
                    true
                | ValueNone -> false
            elif split.Length > 1 then
                if not(String.forall acceptable_data_char split.[1]) then
                    false
                else
                    out <- { Label = split.[0]; Value = ValueSome(split.[1]) }
                    true
            else
                out <- { Label = split.[0]; Value = ValueNone }
                true
        else
            false

    static member FromString(value: string) : Tag =
        match Tag.TryParse(value) with
        | true, result -> result
        | false, _ -> failwithf "Invalid tag: %s" value
