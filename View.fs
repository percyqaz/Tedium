namespace Tedium

open System

type View(state: State) =

    let view = ScreenBuffer(Console.BufferHeight - 1)

    member this.RenderList() : unit =
        let inline front_matter (text: string, is_selected: bool) : string =
            let colored = text.ForeColor(0x666666)
            if is_selected then colored.BackColor(0x333311) else colored

        let inline item_line (item: TodoItem, is_selected: bool) : string =
            let done_marker =
                if item.Done then "x".ForeColor(0x66FF66) else "*".ForeColor(0xFF8888)

            let tags =
                item.Tags |> Seq.map _.ToString() |> String.concat " " |> _.ForeColor(0x66FFFF).Bold()

            let line = sprintf "%s %s %s" done_marker item.Text tags
            if is_selected then line.BackColor(0x666622) else line

        let inline sub_item_line (item: TodoItem, is_selected: bool) : string =
            let done_marker =
                if item.Done then "x".ForeColor(0x66FF66) else "*".ForeColor(0xFF8888)

            let tags =
                item.Tags |> Seq.map _.ToString() |> String.concat " " |> _.ForeColor(0x66FFFF).Bold()

            let line = sprintf "  %s %s %s" done_marker (item.Text.ForeColor(0xAAAAAA)) tags
            if is_selected then line.BackColor(0x333311) else line

        for fm in state.List.FrontMatter do
            view.Line(front_matter(fm, false))

        for item in state.List.Items do
            let is_selected = state.Selected = item

            view.Line(item_line(item, is_selected), is_selected)

            for fm in item.FrontMatter |> Seq.truncate 2 do
                view.Line(front_matter("  " + fm, is_selected))

            if item.FrontMatter.Count >= 3 then
                view.Line(front_matter("  ...", is_selected))

            for sub_item in item.Items |> Seq.truncate 2 do
                view.Line(sub_item_line(sub_item, is_selected))

            if item.Items.Count >= 3 then
                view.Line(front_matter(sprintf "  +%i more" (item.Items.Count - 2), is_selected))

    member this.Redraw() : unit =
        this.RenderList()
        view.Draw()
        Console.Write(state.Buffer.ForeColor(0x88FF88).Bold().ClearRestOfLine())
