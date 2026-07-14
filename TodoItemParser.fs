namespace Tedium

open System
open System.Text

type Indentation =
    {
        Level: int
        FrontMatter: ResizeArray<string>
        Items: ResizeArray<TodoItem>
    }

    static member Create(level: int) : Indentation =
        { Level = level; FrontMatter = ResizeArray(); Items = ResizeArray() }

    static member Create(level: int, item: TodoItem) =
        { Level = level; FrontMatter = item.FrontMatter; Items = item.Items }


type TodoItemParser() =

    let mutable stack: Indentation list = [ Indentation.Create(0) ]

    member private this.TryParseItem(indent: Indentation, line: string) : bool =
        if line.Length > indent.Level && (line.[indent.Level] = 'x' || line.[indent.Level] = '*') then
            let is_done = line.[indent.Level] = 'x'
            let mutable tags = Set.empty

            let words =
                line
                    .Substring(indent.Level + 1)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)

            let text = StringBuilder()

            for w in words do
                match Tag.TryParse(w) with
                | true, tag -> tags <- tags.Add(tag)
                | false, _ -> ignore(text.Append(w + " "))

            if text.Length > 0 then
                let item =
                    {
                        Text = text.ToString().TrimEnd(' ')
                        Done = is_done
                        Tags = tags
                        FrontMatter = ResizeArray()
                        Items = ResizeArray()
                    }

                indent.Items.Add(item)
                stack <- Indentation.Create(indent.Level + 1, item) :: stack
                true
            else
                false
        else
            false

    member private this.TryParseFrontMatter(indent: Indentation, line: string) : bool =
        if line.Substring(0, min line.Length indent.Level).Trim(' ') = "" then
            indent.FrontMatter.Add(line.Substring(indent.Level))
            true
        else
            false

    member this.ParseLine(line: string) : unit =
        let mutable indent = List.head stack

        while not(this.TryParseItem(indent, line) || this.TryParseFrontMatter(indent, line)) do
            stack <- List.tail stack
            indent <- List.head stack

    member this.ToTodoFile(path: string) : TodoFile =
        let base_level = List.last stack
        { Path = path; FrontMatter = base_level.FrontMatter; Items = base_level.Items }