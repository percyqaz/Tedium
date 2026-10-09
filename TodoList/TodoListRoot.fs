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
        let swap_path = this.Path + ".swp"
        let backup_path = this.Path + ".bak"

        File.WriteAllLines(swap_path, TodoItemWriter.WriteElementContents(this.RootElement).ToSeq())

        if File.Exists(this.Path) then
            File.Move(this.Path, backup_path, true)

        File.Move(swap_path, this.Path)
