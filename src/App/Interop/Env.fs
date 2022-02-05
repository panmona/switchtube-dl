module TubeDl.Env

open System

// TODO check this again after a restart
// If I don't get it to work: drop this feature again :-(
let variable name =
    let res = Environment.GetEnvironmentVariable (name, EnvironmentVariableTarget.Machine) // TODO Or machine?
    match res with
    | ""
    | null -> None
    | v -> Some v

let workingDir = Environment.CurrentDirectory
