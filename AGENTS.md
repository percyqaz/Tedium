# Tedium project context

## What this project does

Tedium is a personal, terminal-based hierarchical todo list, packaged as the `tedium` .NET tool. It loads `~/todo.txt` (the current user's profile directory) at startup, lets the user browse and edit the list in a full-screen terminal UI, and saves changes back to that text file. The target framework is .NET 10 and the codebase is F#.

## Todo file format and model

- An active item starts with `* ` and a completed item with `x `; indentation is two spaces per nesting level.
- Item text is followed by zero or more whitespace-separated tags. Tags are `@label` or `@label:value`; labels accept lowercase ASCII letters, digits, `_`, and `-`. Values accept ASCII letters/digits, `_`, `-`, `:`, and `/`.
- Recognized date shorthands such as `@TODAY`, `@TOMORROW`, or `@MONDAY` become `@date:YYYY-MM-DD`; weekday shorthands resolve to the next occurrence (strictly after today).
- Text that is not parsed as an item at the current indentation is retained as front matter/description lines. Items can have nested children and their own front matter.
- The parser and writer are `TodoList/TodoItemParser.fs` and `TodoList/TodoItemWriter.fs`. `TodoListRoot.Load` opens or creates the file; `Save` rewrites it from the in-memory tree. Keep round-trip behavior in mind when changing either.
- Search filters are whitespace-separated keywords and tags; `-` negates a filter and `^tagname` adds a descending sort key. Keyword matching is case-insensitive. Search is scoped to the current list level unless its buffer begins with `*`, which runs the same query globally through every nested item; removing the prefix or leaving global search restores the prior scope.
- Calendar view finds tagged items recursively throughout the tree using exact `@date:YYYY-MM-DD` values. It displays three Sunday-starting weeks.

## UI and commands

- `Program.fs` chooses the todo file path; `Tedium.fs` sets default key mappings and starts the render/input/save loop.
- `CLI/` handles ANSI terminal output, key sequences, command buffering, and search text input. `State/` owns app state, Normal/Search/Calendar modes, and command dispatch. `UI/View.fs` renders those modes. `TodoList/` owns the tree, tags, parsing, writing, filtering, and mutations.
- Navigation keys are `h/j/k/l` (arrow up/down are also aliased). Enter opens, Escape closes/backtracks (and exits from the root normal view), `x`/`X` mark/unmark done, `e` edits, `r` renames, `dd` deletes, `.` edits item contents/description, Tab toggles search input, Alt+h/j/k/l moves items, `G` shows a GitHub issue, and `C` toggles calendar mode.
- Search text is entered in the bottom search prompt. Command buffer messages are submitted with Enter. Built-in messages begin with `:` (for example `:q`, `:up`, `:color_tag @date = ffff99`); a line beginning `*` inserts an item and a line beginning `@` toggles a tag on the selected item.
- Editing invokes external `vim` on a temporary file. GitHub issue lookup invokes the `gh` CLI; issue details use the current scope's `@repo:value` tag and selected item's `@gh:value` tag. These integrations require the relevant executable to be on PATH.
- State saves when marked dirty and the input loop times out, and once more on exit. Calendar mode supports browsing and adding items; several edit/move/delete commands are currently marked NYI there.

## Repository and development notes

- `Tedium.fsproj` explicitly lists all F# source files in compile order. F# declarations must be compiled before files that depend on them, so update this list when adding source files and preserve dependency ordering.
- `Tedium.slnx` is the solution. Build with `dotnet build Tedium.slnx` using the .NET 10 SDK. `update.sh` packs the project and installs/uninstalls the global `tedium` tool from `bin/Release`.
- There is no test project in the repository at present.
- The executable is interactive and expects a real terminal with ANSI support. Some features also rely on `vim` or GitHub CLI (`gh`) as described above.
