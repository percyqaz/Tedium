namespace Tedium

open System

[<Struct>]
type Tag =
    {
        Label: string
        Value: string voption
    }

    override this.ToString() : string =
        match this.Value with
        | ValueSome v -> sprintf "@%s:%s" this.Label v
        | ValueNone -> sprintf "@%s" this.Label

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
                false
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
