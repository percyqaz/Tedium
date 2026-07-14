open Tedium

let p = TodoItemParser()
let text = """
design philosophy: 
I find it difficult to prioritise in absolute terms
so I would rather prioritise relatively

* marks a todo item
 todo items can have any text matter directly below them, conventionally indented with 1 space but optional
 * todo items can have nested items
x marks a completed todo item
* @ followed by [a-z0-9-_:]+ indicates tags
* @work tags can appear anywhere in an item @date:2026-07-14 but get auto-formatted to the end
* uppercase tags reserved as shorthands e.g. @TODAY as shorthand for @date:2026-07-14

j and k to navigate up/down
l to scope into an item, viewing only its text and subtasks
h to unscope out of an item
feature: type @...<Enter> to add a tag instantly
feature: type *...<Enter> to add a todo item instantly
feature: reorder things up/down with alt+k and alt+j
feature: calendar view
feature: select tasks, place them under a task
"""
for line in text.Split("\n") do
    p.ParseLine(line)
printfn "%A" (p.ToTodoFile("C:/todo.txt"))