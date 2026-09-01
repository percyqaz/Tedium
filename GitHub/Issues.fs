namespace Tedium

open System
open System.Text.Json.Serialization

type GithubIdentity =
    {
        [<JsonPropertyName("login")>]
        Login: string
    }

type Label =
    {
        [<JsonPropertyName("name")>]
        Name: string
        [<JsonPropertyName("description")>]
        Description: string
        [<JsonPropertyName("color")>]
        Color: string
    }

type Issue =
    {
        [<JsonPropertyName("assignees")>]
        Assignees: GithubIdentity list
        [<JsonPropertyName("author")>]
        Author: GithubIdentity
        [<JsonPropertyName("body")>]
        Body: string
        [<JsonPropertyName("labels")>]
        Labels: Label list
        [<JsonPropertyName("number")>]
        Number: int
        [<JsonPropertyName("title")>]
        Title: string
        [<JsonPropertyName("updatedAt")>]
        UpdatedAt: DateTime
    }

    member this.Print() : unit =
        Console.WriteLine(
            (sprintf "#%i " this.Number).Bold().ForeColor(0x88ff88) + this.Title.Bold().ForeColor(0xffffff)
        )

        Console.Write((this.UpdatedAt.ToShortDateString() + " by " + this.Author.Login + " ").ForeColor(0x666666))

        let inline label_to_string (label: Label) =
            label.Name.Replace(" ", "").ForeColor(Convert.ToInt32(label.Color, 16))

        Console.WriteLine(String.concat " " (this.Labels |> Seq.map label_to_string))

        Console.WriteLine(this.Body)
