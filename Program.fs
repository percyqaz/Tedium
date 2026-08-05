open System
open System.IO
open Tedium

let TODO_PATH =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "todo.txt")

Tedium.loop(TODO_PATH)
