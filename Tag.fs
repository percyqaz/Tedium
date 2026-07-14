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
        let inline is_acceptable_character (c: char) =
            Char.IsAsciiDigit(c) || Char.IsAsciiLetterLower(c) || c = '_' || c = '-' || c = ':'

        if value.StartsWith('@') then
            let value = value.Substring(1)

            if String.forall is_acceptable_character value then
                let split = value.Split(':', 2)
                out <- { Label = split.[0]; Value = if split.Length > 1 then ValueSome(split.[1]) else ValueNone }
                true
            else
                false
        else
            false

    static member FromString(value: string) : Tag =
        match Tag.TryParse(value) with
        | true, result -> result
        | false, _ -> failwithf "Invalid tag: %s" value
