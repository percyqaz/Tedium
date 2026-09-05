namespace Tedium

open System

type Filter =
    | Keyword of string
    | Tag of Tag
    | Invert of Filter

    member this.Matches(item: TodoItem) : bool =
        match this with
        | Keyword word -> item.Text.Contains(word, StringComparison.OrdinalIgnoreCase)
        | Tag tag ->
            match item.GetTagValue(tag.Label) with
            | None -> false
            | Some v when tag.Value.IsSome -> tag.Value = v
            | _ -> true
        | Invert f -> not(f.Matches(item))

type SearchQuery =
    {
        Filters: Filter list
        Sort: string list
    }

    static member Parse(text: string) : SearchQuery =
        let words = text.Split(' ', StringSplitOptions.TrimEntries)

        let rec inline parse_filter (word: string) : Filter =
            if word.StartsWith('-') then
                Invert(parse_filter(word.Substring(1)))
            elif word.StartsWith('@') then
                match Tag.TryParse(word) with
                | true, tag -> Tag(tag)
                | false, _ -> Keyword(word)
            else
                Keyword(word)

        let filters = ResizeArray()
        let sorts = ResizeArray()

        let parse (word: string) : unit =
            if word.StartsWith('^') then sorts.Add(word.Substring(1)) else filters.Add(parse_filter(word))

        for word in words do
            parse(word)

        { Filters = List.ofSeq filters; Sort = List.ofSeq sorts }

    member this.SortKey(item: TodoElement) : string list =
        let inline get_key (tag_name: string) : string =
            match item.GetTagValue(tag_name) with
            | None -> ""
            | Some ValueNone -> " "
            | Some(ValueSome v) -> if String.forall Char.IsAsciiDigit v then v.PadLeft(6, '0') else v

        this.Sort |> List.map get_key

    member this.Apply(items: ResizeArray<TodoElement>) : TodoElement array =

        let filtered =
            items
            |> Seq.filter(fun item ->
                match item.Guts with
                | Item i -> this.Filters |> List.forall _.Matches(i)
                | File _ -> false
            )

        if this.Sort <> [] then
            filtered |> Seq.sortByDescending this.SortKey |> Array.ofSeq
        else
            filtered |> Array.ofSeq
