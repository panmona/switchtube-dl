module TubeDl.Program

open Argu
open TubeDl.Cli

let runConfig c =
    // TODO implement config command handling
    Ok ()


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
        ArgumentParser.Create<CliArgs> (errorHandler = errorHandler)

    let results =
        parser.ParseCommandLine (inputs = argv, raiseOnUsage = true)

    let executionType = CliArgParse.tryGetExecutionType results

    match executionType with
    | Some (ExecutionType.Config c) -> runConfig c
    | Some (ExecutionType.Download d) -> DownloadCmd.runDownload d
    | None ->
        // TODO Print these in red with proper error message
        printfn "Couldn't determine execution type. Specify a subcommand."
        printfn "%s" (parser.PrintUsage ())
        Error (ArgumentsNotSpecified "")
    |> CliError.getExitCode
