namespace Tedium

open System
open System.Drawing
open System.Runtime.CompilerServices
open System.Text

type AnsiStringExtensions =

    [<Extension>]
    static member ForeColor(text: string, foreground: Color) : string =
        sprintf "\u001b[38;2;%d;%d;%dm%s\u001b[39m" foreground.R foreground.G foreground.B text

    [<Extension>]
    static member ForeColor(text: string, foreground: int) : string =
        text.ForeColor(Color.FromArgb(foreground))

    [<Extension>]
    static member BackColor(text: string, background: Color) : string =
        sprintf "\u001b[48;2;%d;%d;%dm%s\u001b[49m" background.R background.G background.B text

    [<Extension>]
    static member BackColor(text: string, background: int) : string =
        text.BackColor(Color.FromArgb(background))

    [<Extension>]
    static member Bold(text: string) : string = sprintf "\u001b[1m%s\u001b[22m" text

    [<Extension>]
    static member ClearRestOfLine(text: string) : string = sprintf "%s\u001b[K" text

type ScreenBuffer(height: int) =

    let lines = ResizeArray()
    let mutable cursor = 0
    let mutable scroll_position = 0

    member val ScrollOff = 6
    member val LinesBelow = 1
    member val Height = height

    member this.CursorHere() : unit = cursor <- lines.Count

    member this.Line(line: string) : unit = lines.Add(line)

    member this.Line(line: string, cursor_here: bool) : unit =
        if cursor_here then
            this.CursorHere()

        this.Line(line)

    member this.Draw() : unit =
        let sb = StringBuilder().Append("\u001b[H")

        let top_of_requested_view = max 0 (cursor - this.ScrollOff)

        if top_of_requested_view < scroll_position then
            scroll_position <- top_of_requested_view

        let bottom_of_requested_view =
            min (lines.Count - 1 + this.LinesBelow) (cursor + this.ScrollOff)

        if bottom_of_requested_view - this.Height + 1 > scroll_position then
            scroll_position <- bottom_of_requested_view - this.Height + 1

        let mutable index = scroll_position

        for i = 1 to this.Height do
            let line = if index < lines.Count then lines.[index] else ""
            sb.AppendLine(line.ClearRestOfLine()) |> ignore
            index <- index + 1

        Console.Write(sb.ToString())

        lines.Clear()
