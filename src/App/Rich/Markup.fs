module TubeDl.Rich.Markup

open Spectre.Console

let print markup =
    AnsiConsole.MarkupLine markup

let log msg =
    print $"[grey50]LOG:[/] %s{msg}[grey50]...[/]"
