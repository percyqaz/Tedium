namespace Tedium

type TodoItemWriter() =

    let output = ResizeArray<string>()

    member private this.WriteElement(indent: int, item: TodoElement) : unit =
        let padding = String.replicate indent " "
        output.Add(padding + item.ToString())
        this.WriteElementContents(indent + 1, item)

    member private this.WriteElementContents(indent: int, element: TodoElement) : unit =
        if not(element.Guts.IsFile) then
            let padding = String.replicate indent " "

            for f in element.FrontMatter do
                output.Add(padding + f)

            for item in element.Items do
                this.WriteElement(indent, item)

    member this.WriteElement(element: TodoElement) : unit = this.WriteElement(0, element)

    member this.WriteElementContents(element: TodoElement) : unit = this.WriteElementContents(0, element)

    static member WriteElement(element: TodoElement) : TodoItemWriter =
        let writer = TodoItemWriter()
        writer.WriteElement(element)
        writer

    static member WriteElementContents(element: TodoElement) : TodoItemWriter =
        let writer = TodoItemWriter()
        writer.WriteElementContents(element)
        writer

    override this.ToString() : string = String.concat "\n" output + "\n"

    member this.ToSeq() : string seq = output
