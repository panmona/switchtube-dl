module TubeDl.Rich.Markup

open Spectre.Console

let print markup = AnsiConsole.Markup markup

let printn markup = AnsiConsole.MarkupLine markup

let log msg =
    printn $"[grey50]LOG:[/] %s{msg}[grey50]...[/]"

let eprintn markup =
    let settings = AnsiConsoleSettings ()
    // Prevent garbled output in redirected terminal.
    // Automatic Spectre handling doesn't seem to work for that case even though it seems to have been implemented
    // TODO create an issue for that. Also doesn't work with Spinner output for normal Output Out
    match System.Console.IsErrorRedirected with
    | true -> settings.Ansi <- AnsiSupport.No
    | false -> settings.Ansi <- AnsiSupport.Detect

    settings.Out <- AnsiConsoleOutput System.Console.Error
    AnsiConsole.Console <- AnsiConsole.Create settings

    AnsiConsole.MarkupLine markup
