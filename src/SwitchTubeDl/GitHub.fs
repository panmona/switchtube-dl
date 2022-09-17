[<RequireQualifiedAccess>]
module TubeDl.GitHub

let url = "https://github.com/panmona/switchtube-dl/issues/new"

let createIssue =
    $"Please create a helpful issue at [yellow bold underline]%s{url}[/]"
