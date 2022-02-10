module TubeDl.ParseRange

let tokens =
    String.replace " " "" videosToDownload
    |> Text.split [| ',' |]
    |> List.ofArray

let parseTokens (tokens : string list) =
    let parsedTokens =
        tokens
        |> List.map (fun t ->
            match t with
            | Int i -> Ok [ i ]
            | Range r -> Ok r
            | _ -> Error t
        )

    // | _, Error errs | Error errs, _
    let f state item =
        match state, item with
        | Ok prevResults, Ok res -> Ok (prevResults @ res)
        | Ok _, Error errs -> Error [ errs ]
        | Error errs, Ok _ -> Error errs
        | Error prevErrs, Error errs -> Error (prevErrs @ [ errs ])

    parsedTokens |> List.fold f (Ok [])
