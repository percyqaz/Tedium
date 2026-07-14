namespace Tedium

type TodoItemWriter() =

    let output = ResizeArray<string>()

    member private this.WriteItem(indent: int, item: TodoItem) : unit =
        let padding = String.replicate indent " "
        output.Add(padding + item.ToString())
        this.WriteFileContents(indent + 1, item.FrontMatter, item.Items)

    member private this.WriteFileContents(indent: int, front_matter: string seq, items: TodoItem seq) : unit =
        let padding = String.replicate indent " "

        for f in front_matter do
            output.Add(padding + f)

        for item in items do
            this.WriteItem(indent, item)

    member this.WriteFileContents(front_matter: string seq, items: TodoItem seq) : unit =
        this.WriteFileContents(0, front_matter, items)

    member this.WriteFileContents(file: TodoFile) : unit =
        this.WriteFileContents(file.FrontMatter, file.Items)

    override this.ToString() : string = String.concat "\n" output + "\n"
