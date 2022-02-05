namespace TubeDl.Cli

// TODO improve these types
type RunError = | ScriptFailed of ErrorMessage : string

type CliError =
    | ArgumentsNotSpecified of ErrorMessage : string
    | RunErr of RunError

module CliError =
    let getExitCode result =
        match result with
        | Ok () -> 0
        | Error err ->

        match err with
        | ArgumentsNotSpecified _ -> 1
        | RunErr runErr ->

        match runErr with
        | ScriptFailed _ -> 2
