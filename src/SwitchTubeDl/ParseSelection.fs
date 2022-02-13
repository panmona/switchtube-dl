module TubeDl.ParseSelection

open Microsoft.FSharpLu
open FsToolkit.ErrorHandling

open TubeDl.Int

let (|Range|_|) str =
    let range = Text.split [| '-' |] str

    match range with
    | [| Int l ; Int r |] when l < r -> Some [ l..r ]
    | _ -> None

let private parse tokens =
    tokens
    |> List.map (fun t ->
        match t with
        | Int i -> Ok [ i ]
        | Range r -> Ok r
        | _ -> Error t
    )
    |> List.distinct
    |> List.fold Folder.allErrorOrAllOk (Ok [])

let tryParseSelection str =
    let tokens =
        String.replace " " "" str
        |> Text.split [| ',' |]
        |> List.ofArray

    parse tokens

type ValidationResult =
    | Valid
    | MaxValue of int
    | MinValue of int
    | InvalidTokens of string list

let isValidAndInRange (min, max) str =
    let selRes = tryParseSelection str

    match selRes with
    | Ok sel ->
        let selMax = List.max sel
        let selMin = List.min sel

        match selMax <= max, selMin >= min with
        | true, true -> Valid
        | false, _ -> MaxValue selMax
        | _, false -> MinValue selMin
    | Error e -> InvalidTokens e
