namespace Tedium

[<RequireQualifiedAccess>]
type Mode =
    | Normal of NormalMode
    // Search
    // GlobalSearch

    member this.Root: TodoListRoot =
        match this with
        | Normal nm -> nm.Root

    member this.Selected: TodoElement option =
        match this with
        | Normal nm -> nm.Selected

    member this.Selection: int option =
        match this with
        | Normal nm -> nm.Selection
