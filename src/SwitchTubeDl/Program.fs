module TubeDl.Program

open Argu

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

    let parser = ArgumentParser.Create<CliArgs> (errorHandler = errorHandler)

    let results = parser.ParseCommandLine (inputs = argv, raiseOnUsage = true)

    Download.runDownload results
    |> CliError.getExitCode
