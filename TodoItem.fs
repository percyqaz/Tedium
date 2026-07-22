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

type TodoFile = { Path: string }

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
        | Item item ->
            if item.Tags.Contains(tag) then
                item.Tags <- item.Tags.Remove(tag)
            else
                item.Tags <- item.Tags.Add(tag)
        | _ -> ()

    member this.MarkDone() : unit =
        match this.Guts with
        | Item item -> item.Done <- true
        | _ -> ()

    member this.UnmarkDone() : unit =
        match this.Guts with
        | Item item -> item.Done <- false
        | _ -> ()
