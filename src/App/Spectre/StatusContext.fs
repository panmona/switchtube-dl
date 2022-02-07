module TubeDl.StatusContext

open Spectre.Console

let spinner ctx spinner =
    StatusContextExtensions.Spinner (ctx, spinner)
    |> ignore

let status ctx msg =
    StatusContextExtensions.Status (ctx, msg)
    |> ignore
