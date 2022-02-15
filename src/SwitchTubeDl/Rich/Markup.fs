namespace TubeDl.Rich

open Spectre.Console

module Markup =
    let print markup = AnsiConsole.Markup markup

    let printn markup = AnsiConsole.MarkupLine markup

    let log msg =
        printn $"[grey50]LOG:[/] %s{msg}[grey50]...[/]"

    let eprintn markup =
        let settings = AnsiConsoleSettings ()
        settings.Out <- AnsiConsoleOutput System.Console.Error
        AnsiConsole.Console <- AnsiConsole.Create settings

        AnsiConsole.MarkupLine markup

    let escape markup = Markup.Escape markup

[<AutoOpen>]
module Escape =
    /// Escape possible markup of a string
    let esc (markup : string) = Markup.escape markup
