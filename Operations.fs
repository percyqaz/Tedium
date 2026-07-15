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
        dispatch_shell_command("vim '" + tmp + "'")
        File.ReadAllLines(tmp)

    let edit_fm (fm: ResizeArray<string>, items: ResizeArray<TodoItem>) : unit =
        let parsed = TodoItemParser.ParseLines(edit_with_vim(fm)).ToTodoFile("")
        fm.Clear()
        fm.AddRange(parsed.FrontMatter)
        items.InsertRange(0, parsed.Items)

    let edit_fm_file (file: TodoFile) : unit = edit_fm(file.FrontMatter, file.Items)
    let edit_fm_item (item: TodoItem) : unit = edit_fm(item.FrontMatter, item.Items)
