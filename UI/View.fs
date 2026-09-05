namespace Tedium

open System

type View(state: State) =

    let view = ScreenBuffer(Console.BufferHeight - 3)

    member this.ElementLine(element: TodoElement, is_selected: bool) : string =
        let inline item_line (item: TodoItem, element: TodoElement) : string =
            let done_marker =
                if item.Done then "x".ForeColor(0x66FF66) else "*".ForeColor(0xFF8888)

            let more_items_marker =
                if element.Items.Count > 0 then " +".ForeColor(0x8888FF) else ""

            let description_marker =
                if element.FrontMatter.Count > 0 then " ...".ForeColor(0x88FFFF) else ""

            let inline format_tag (tag: Tag) =
                let color =
                    match Map.tryFind tag.Label state.TagColors with
                    | Some i -> i
                    | None -> 0x66FFFF

                (" " + tag.ToString()).ForeColor(color)

            let tags = item.Tags |> Seq.map format_tag |> String.concat "" |> _.Bold()

            let line =
                sprintf "%s %s%s%s%s" done_marker item.Text tags more_items_marker description_marker

            if is_selected then line.BackColor(0x666622) else line

        let inline file_line (file: TodoFile) : string =
            let file_marker = "/".ForeColor(0x6666FF).Bold()

            let line = sprintf "%s %s" file_marker file.Path
            if is_selected then line.BackColor(0x666622) else line

        match element.Guts with
        | Item item -> item_line(item, element)
        | File file -> file_line(file)

    member this.RenderList(nm: NormalMode) : unit =

        for item in nm.Scope.Items do
            let is_selected = nm.Selected = Some item

            view.Line(this.ElementLine(item, is_selected), is_selected)

    member this.RenderFrontMatter(nm: NormalMode) : unit =
        let is_selected = nm.Selected = None

        let inline front_matter (text: string) : string =
            let colored = text.ForeColor(0x666666)
            if is_selected then colored.BackColor(0x333311) else colored

        if is_selected then
            view.CursorHere()

        for fm in nm.Scope.FrontMatter do
            view.Line(front_matter(fm).ClearRestOfLine())

    member this.RenderNormalMode(nm: NormalMode) : unit =
        let tagline =
            let loc = nm.Scope.ToString().ForeColor(0xFF8888)
            sprintf "%s (%i)" loc nm.Scope.Items.Count

        Console.WriteLine(tagline.ClearRestOfLine())
        this.RenderFrontMatter(nm)
        this.RenderList(nm)

    member this.Redraw() : unit =
        view.Height <- Console.BufferHeight - 3
        Console.Write(AnsiCodes.CursorToOrigin)

        match state.Mode with
        | Mode.Normal nm -> this.RenderNormalMode(nm)

        view.Draw()

        Console.WriteLine("Tedium ".ForeColor(0xFF8888).Bold() + state.StatusLine.ForeColor(0x444444).ClearRestOfLine())
        Console.Write(state.CommandBuffer.ToString().ForeColor(0x88FF88).Bold().ClearRestOfLine())
