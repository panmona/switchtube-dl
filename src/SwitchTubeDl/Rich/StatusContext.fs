module TubeDl.Rich.StatusContext

open Spectre.Console

let setSpinner ctx spinner =
    StatusContextExtensions.Spinner (ctx, spinner) |> ignore

let setStatus ctx msg =
    StatusContextExtensions.Status (ctx, msg) |> ignore
