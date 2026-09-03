namespace Tedium

open System.Diagnostics
open System.Text.Json

module GitHub =
    // load issues list from repo
    //  in this view: close, reopen, view, edit body and title, assign, create new
    // load particular issue from repo
    //  in this view: close, reopen, edit body and title, assign, back to list

    let private run_gh_command (command: string) : Result<string, string> =
        let start_info =
            ProcessStartInfo(
                "gh",
                command,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            )

        use proc = Process.Start(start_info)
        let read_output = proc.StandardOutput.ReadToEndAsync()
        let read_error = proc.StandardError.ReadToEndAsync()
        proc.WaitForExit()
        let output = read_output.GetAwaiter().GetResult().Trim()
        let error = read_error.GetAwaiter().GetResult().Trim()

        if proc.ExitCode <> 0 then Error(if output = "" then error else output) else Ok(output)

    let get_issues (repo: string) : Result<Issue array, string> =
        run_gh_command("issue list -L 100 --json assignees,author,body,labels,number,title,updatedAt --repo " + repo)
        |> Result.map JsonSerializer.Deserialize<Issue array>

    let get_issue (repo: string, number: string) : Result<Issue, string> =
        run_gh_command(
            "issue view --json assignees,author,body,labels,number,title,updatedAt --repo " + repo + " " + number
        )
        |> Result.map JsonSerializer.Deserialize<Issue>
