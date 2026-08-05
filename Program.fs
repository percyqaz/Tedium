open System
open System.IO
open Tedium

let TODO_PATH =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "todo.txt")

Interactive.loop(TODO_PATH)
