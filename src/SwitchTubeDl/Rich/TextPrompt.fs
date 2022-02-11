module TubeDl.Rich.TextPrompt

open Spectre.Console

let promptWithValidation msg validator =
    let prompt = TextPrompt msg

    let validation =
        validator
        >> function
            | Ok () -> ValidationResult.Success ()
            | Error msg -> ValidationResult.Error msg

    prompt.Validate validation |> ignore

    AnsiConsole.Prompt prompt
