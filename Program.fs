open System
open System.IO
open Tedium

let config = ConfigFile.FromUserFile(".tedium")

let TODO_PATH =
    match config.Directives.TryGetValue("path") with
    | true, value -> value
    | false, _ -> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "todo.txt")

Tedium.loop(TODO_PATH, config.Commands)
