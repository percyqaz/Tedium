namespace Tedium

type TodoItem =
    {
        mutable Text: string
        mutable Done: bool
        mutable Tags: Set<Tag>
    }

    override this.ToString() : string =
        let marker = if this.Done then 'x' else '*'

        let tags =
            if this.Tags.IsEmpty then "" else " " + (this.Tags |> Seq.map _.ToString() |> String.concat " ")

        sprintf "%c %s%s" marker this.Text tags

    member this.ToggleTag(tag: Tag) : unit =
        match this.Tags |> Seq.tryFind(fun t -> t.Label = tag.Label) with
        | Some tag_already_added ->
            this.Tags <-
                if tag.Value <> ValueNone then
                    this.Tags.Remove(tag_already_added).Add(tag)
                else
                    this.Tags.Remove(tag_already_added)
        | None -> this.Tags <- this.Tags.Add(tag)

    member this.GetTagValue(label: string) : string ValueOption Option =
        this.Tags |> Seq.tryFind(fun t -> t.Label = label) |> Option.map _.Value

type TodoFile =
    {
        Path: string
    }

    override this.ToString() : string = "/ " + this.Path

type TodoElementGuts =
    | Item of TodoItem
    | File of TodoFile

    override this.ToString() : string =
        match this with
        | Item item -> item.ToString()
        | File file -> file.ToString()

type TodoElement =
    {
        Guts: TodoElementGuts
        FrontMatter: ResizeArray<string>
        Items: ResizeArray<TodoElement>
    }

    override this.ToString() : string = this.Guts.ToString()

    member this.ToggleTag(tag: Tag) : unit =
        match this.Guts with
        | Item item -> item.ToggleTag(tag)
        | _ -> ()

    member this.GetTagValue(label: string) : string ValueOption Option =
        match this.Guts with
        | Item item -> item.GetTagValue(label)
        | File _ -> None

    member this.MarkDone() : unit =
        match this.Guts with
        | Item item -> item.Done <- true
        | File _ -> ()

    member this.UnmarkDone() : unit =
        match this.Guts with
        | Item item -> item.Done <- false
        | File _ -> ()
