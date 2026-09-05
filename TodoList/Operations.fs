namespace Tedium

open System
open System.Diagnostics
open System.IO

module Operations =

    let private dispatch_shell_command (command: string) : unit =
        let shell, args =
            if OperatingSystem.IsWindows() then "cmd.exe", "/c " + command else "/bin/sh", "-c \"" + command + "\""

        let start_info = ProcessStartInfo(shell, args)
        Console.Write("\u001b[?1049l\u001b[47h\u001b[2J\u001b[H")
        let proc = Process.Start(start_info)
        proc.WaitForExit()

        Console.Write("\u001b[47l\u001b[?1049h")

    let private edit_with_vim (lines: string seq) : string seq =
        let tmp = Path.GetTempFileName().Replace("\\", "/")
        File.WriteAllLines(tmp, lines)
        dispatch_shell_command("vim -c '+normal! gg$' -c 'set statusline=%=' '" + tmp + "'")
        File.ReadAllLines(tmp)

    let edit_frontmatter (element: TodoElement) : unit =
        let parsed =
            TodoItemParser.ParseLines(edit_with_vim(element.FrontMatter)).ToTodoFile("")

        element.FrontMatter.Clear()
        element.FrontMatter.AddRange(parsed.FrontMatter)
        element.Items.InsertRange(0, parsed.Items)

    let edit_contents (element: TodoElement) : unit =
        let parsed =
            TodoItemParser
                .ParseLines(edit_with_vim(TodoItemWriter.WriteElementContents(element).ToSeq()))
                .ToTodoFile("")

        element.FrontMatter.Clear()
        element.FrontMatter.AddRange(parsed.FrontMatter)
        element.Items.Clear()
        element.Items.AddRange(parsed.Items)

    let edit_name (parent: TodoElement, item: TodoElement) : unit =
        let parsed =
            TodoItemParser.ParseLines(edit_with_vim([ item.ToString() ])).ToTodoFile("")

        parent.FrontMatter.AddRange(parsed.FrontMatter)
        let index = parent.Items.IndexOf(item)
        parent.Items.RemoveAt(index)

        if parsed.Items.Count > 0 then
            parent.Items.Insert(index, { item with Guts = parsed.Items.[0].Guts })
    // todo: what if that turned it into a file

    let edit (parent: TodoElement, item: TodoElement) : unit =
        let parsed =
            TodoItemParser.ParseLines(edit_with_vim(TodoItemWriter.WriteElement(item).ToSeq())).ToTodoFile("")

        parent.FrontMatter.AddRange(parsed.FrontMatter)
        let index = parent.Items.IndexOf(item)
        parent.Items.RemoveAt(index)
        parent.Items.InsertRange(index, parsed.Items)
