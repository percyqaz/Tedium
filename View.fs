namespace Tedium

open System

type View(state: State) =

    let view = ScreenBuffer(Console.BufferHeight - 1)

    member this.RenderFrontMatter() : unit =
        let is_selected = state.Selected = None

        let inline front_matter (text: string) : string =
            let colored = text.ForeColor(0x666666)
            if is_selected then colored.BackColor(0x333311) else colored

        if is_selected then
            view.CursorHere()

        for fm in state.Scope.FrontMatter do
            view.Line(front_matter(fm).ClearRestOfLine())

    member this.ElementLine(element: TodoElement, is_selected: bool) : string =
        let inline item_line (item: TodoItem) : string =
            let done_marker =
                if item.Done then "x".ForeColor(0x66FF66) else "*".ForeColor(0xFF8888)

            let tags =
                item.Tags |> Seq.map _.ToString() |> String.concat " " |> _.ForeColor(0x66FFFF).Bold()

            let line = sprintf "%s %s %s" done_marker item.Text tags
            if is_selected then line.BackColor(0x666622) else line

        let inline file_line (file: TodoFile) : string =
            let file_marker = "/".ForeColor(0x6666FF).Bold()

            let line = sprintf "%s %s" file_marker file.Path
            if is_selected then line.BackColor(0x666622) else line

        match element.Guts with
        | Item item -> item_line(item)
        | File file -> file_line(file)

    member this.SubElementLine(element: TodoElement, is_selected: bool) : string =
        let inline sub_item_line (item: TodoItem) : string =
            let done_marker =
                if item.Done then "x".ForeColor(0x66FF66) else "*".ForeColor(0xFF8888)

            let tags =
                item.Tags |> Seq.map _.ToString() |> String.concat " " |> _.ForeColor(0x66FFFF).Bold()

            let line = sprintf "  %s %s %s" done_marker (item.Text.ForeColor(0xAAAAAA)) tags
            if is_selected then line.BackColor(0x333311) else line

        let inline sub_file_line (file: TodoFile) : string =
            let file_marker = "/".ForeColor(0x6666FF).Bold()

            let line = sprintf "  %s %s" file_marker file.Path
            if is_selected then line.BackColor(0x333311) else line

        match element.Guts with
        | Item item -> sub_item_line(item)
        | File file -> sub_file_line(file)

    member this.RenderList() : unit =
        let inline front_matter (text: string, is_selected: bool) : string =
            let colored = text.ForeColor(0x666666)
            if is_selected then colored.BackColor(0x333311) else colored

        for item in state.Scope.Items do
            let is_selected = state.Selected = Some item

            view.Line(this.ElementLine(item, is_selected), is_selected)

            for fm in item.FrontMatter |> Seq.truncate 2 do
                view.Line(front_matter("  " + fm, is_selected))

            if item.FrontMatter.Count >= 3 then
                view.Line(front_matter("  ...", is_selected))

            for sub_item in item.Items |> Seq.truncate 2 do
                view.Line(this.SubElementLine(sub_item, is_selected))

            if item.Items.Count >= 3 then
                view.Line(front_matter(sprintf "  +%i more" (item.Items.Count - 2), is_selected))

    member this.TagLine() : string =
        let loc = state.Scope.ToString().ForeColor(0xFF8888)
        sprintf "%s (%i)" loc state.Scope.Items.Count

    member this.Redraw() : unit =
        Console.Write("\u001b[H")
        Console.WriteLine(this.TagLine().ClearRestOfLine())
        view.Height <- Console.BufferHeight - 2
        this.RenderFrontMatter()
        this.RenderList()
        view.Draw()
        Console.Write(state.Buffer.ForeColor(0x88FF88).Bold().ClearRestOfLine())
