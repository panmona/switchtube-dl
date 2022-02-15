module TubeDl.Program

open Argu

open TubeDl
open TubeDl.Cli
open TubeDl.Download

[<EntryPoint>]
let main argv =
    let errorHandler =
        ProcessExiter (
            colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some System.ConsoleColor.Red
        )

    let parser =
        ArgumentParser.Create<CliArgs> (
            errorHandler = errorHandler,
            helpTextMessage = "A simple CLI for downloading videos from SwitchTube."
        )

    let results =
        parser.ParseCommandLine (inputs = argv, raiseOnUsage = true)

    match results.Contains CliArgs.Version with
    | true ->
        printfn $"%s{Version.parseVersion ()}"
        0
    | false ->
        Download.runDownload results
        |> CliError.getExitCode
