namespace Tedium

type TodoItem =
    {
        mutable Text: string
        mutable Done: bool
        mutable Tags: Set<Tag>
        FrontMatter: ResizeArray<string>
        Items: ResizeArray<TodoItem>
    }

    override this.ToString() : string =
        let marker = if this.Done then 'x' else '*'

        let tags =
            if this.Tags.IsEmpty then "" else " " + (this.Tags |> Seq.map _.ToString() |> String.concat " ")

        sprintf "%c %s%s" marker this.Text tags

type TodoFile = { Path: string; FrontMatter: ResizeArray<string>; Items: ResizeArray<TodoItem> }
