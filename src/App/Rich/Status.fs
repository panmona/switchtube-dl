module TubeDl.Rich.Status

open Spectre.Console

let setSpinner status spinner =
    StatusExtensions.Spinner (status, spinner)
    |> ignore

let start msg spinner callbackTask =
    let status = AnsiConsole.Status ()
    setSpinner status spinner

    status.StartAsync(msg, func=callbackTask)
    |> Async.AwaitTask

let startDefault msg callbackTask =
    start msg Spinner.Known.Default callbackTask
