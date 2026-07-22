open System
open System.IO
open Tedium

let TODO_PATH =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "todo.txt")

let save (todo_list: TodoElement) : unit =
    File.WriteAllLines(TODO_PATH, TodoItemWriter.WriteElementContents(todo_list).ToSeq())

let load () : TodoElement =
    use file = File.Open(TODO_PATH, FileMode.OpenOrCreate)
    use sr = new StreamReader(file)
    let p = TodoItemParser()

    while not(sr.EndOfStream) do
        p.ParseLine(sr.ReadLine())

    p.ToTodoFile(TODO_PATH)

let todo_list = load()
Interactive.loop(todo_list)
save(todo_list)
