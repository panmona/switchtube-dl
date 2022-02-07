module TubeDl.Rich.TextPrompt

open Spectre.Console

let promptWithValidation msg validator =
    let prompt = TextPrompt msg

    let validation =
        let toUnelevated =
            function
            | Ok x
            | Error x -> x

        validator
        >> Result.map ValidationResult.Success
        >> Result.mapError (fun r -> ValidationResult.Error r)
        >> toUnelevated

    prompt.Validate validation |> ignore

    AnsiConsole.Prompt prompt
