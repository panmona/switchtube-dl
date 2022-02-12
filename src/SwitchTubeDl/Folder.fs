module TubeDl.Folder

let allErrorOrAllOk state item =
    match state, item with
    | Ok prevResults, Ok res -> Ok (prevResults @ res)
    | Ok _, Error errs -> Error [ errs ]
    | Error errs, Ok _ -> Error errs
    | Error prevErrs, Error errs -> Error (prevErrs @ [ errs ])

/// Folder for List of Results with single items in the Ok case. Returns only the first error
let firstErrorFolderSingle state item =
    match state, item with
    | Ok prev, Ok res -> Ok (prev @ [ res ])
    | Error errs, _
    | _, Error errs -> Error errs

/// Folder for List of Results with a list of items in the Ok case. Returns only the first error
let firstErrorFolderList state item =
    match state, item with
    | Ok prevResults, Ok res -> Ok (prevResults @ res)
    | Error errs, _
    | _, Error errs -> Error errs
