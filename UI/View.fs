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

    member this.RenderList(items: TodoElement seq, selected: TodoElement option) : unit =

        for item in items do
            let is_selected = Some item = selected

            view.Line(this.ElementLine(item, is_selected), is_selected)

    member this.RenderFrontMatter(fm: ResizeArray<string>, selected: TodoElement option) : unit =
        let is_selected = selected = None

        let inline front_matter (text: string) : string =
            let colored = text.ForeColor(0x666666)
            if is_selected then colored.BackColor(0x333311) else colored

        if is_selected then
            view.CursorHere()

        for line in fm do
            view.Line(front_matter(line).ClearRestOfLine())

    member this.RenderNormalMode(nm: NormalMode) : unit =
        view.Height <- Console.BufferHeight - 3

        let tagline =
            let loc = nm.Scope.ToString().ForeColor(0xFF8888)
            sprintf "%s (%i)" loc nm.Items.Count

        Console.WriteLine(tagline.ClearRestOfLine())
        this.RenderFrontMatter(nm.Scope.FrontMatter, nm.Selected)
        this.RenderList(nm.Items, nm.Selected)
        view.Draw()

    member this.RenderSearchMode(sm: SearchMode) : unit =
        view.Height <- Console.BufferHeight - 3

        let tagline =
            let loc = sm.Scope.ToString().ForeColor(0xFF8888)
            sprintf "%s (%i results for: %O)" loc sm.Results.Length state.SearchBuffer

        Console.WriteLine(tagline.ClearRestOfLine())
        this.RenderFrontMatter(sm.Scope.FrontMatter, sm.Selected)
        this.RenderList(sm.Results, sm.Selected)
        view.Draw()

    member this.Redraw() : unit =
        Console.Write(AnsiCodes.CursorToOrigin)

        match state.Mode with
        | Mode.Normal nm -> this.RenderNormalMode(nm)
        | Mode.Search sm -> this.RenderSearchMode(sm)

        Console.WriteLine("Tedium ".ForeColor(0xFF8888).Bold() + state.StatusLine.ForeColor(0x444444).ClearRestOfLine())

        if state.SearchBufferFocused then
            Console.Write(state.SearchBuffer.ToString().ForeColor(0x8888FF).Bold().ClearRestOfLine())
        else
            Console.Write(state.CommandBuffer.ToString().ForeColor(0x88FF88).Bold().ClearRestOfLine())
