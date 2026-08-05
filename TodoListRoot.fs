namespace Tedium

open System.IO

type TodoListRoot =
    {
        Path: string
        RootElement: TodoElement
    }

    static member Load(path: string) : TodoListRoot =
        use file = File.Open(path, FileMode.OpenOrCreate)
        use sr = new StreamReader(file)
        let p = TodoItemParser()

        while not(sr.EndOfStream) do
            p.ParseLine(sr.ReadLine())

        { Path = path; RootElement = p.ToTodoFile(path) }

    member this.Save() : unit =
        File.WriteAllLines(this.Path, TodoItemWriter.WriteElementContents(this.RootElement).ToSeq())
