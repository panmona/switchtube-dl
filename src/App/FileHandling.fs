module TubeDl.FileHandling

open System.IO
open FsHttp.Helper
open Microsoft.FSharpLu

// TODO move this elsewhere
[<RequireQualifiedAccess>]
type OverwriteFile =
    | KeepAsIs
    | Overwrite

[<RequireQualifiedAccess>]
type SaveFileError = | FileExists

// TODO move these functions to a proper module.
// -> maybe map all api objects to proper internal objects after or in the Decoder instead?
let fileNameFromTitle videoTitle =
    let isInvalidChar c =
        // This list may be incomplete
        [
            '"'
            '<'
            '>'
            '|'
            ','
            '\a'
            '\b'
            '\t'
            '\n'
            '\v'
            '\f'
            '\r'
            ':'
            '*'
            '?'
            '\\'
            '/'
        ]
        |> List.contains c

    let replaceSpace =
        function
        | ' ' -> '_'
        | c -> c

    // TODO ö, ä etc.
    String.toCharArray videoTitle
    |> Array.filter (isInvalidChar >> not)
    |> Array.map replaceSpace
    |> System.String

let private saveFile fullPath (stream : Stream) =
    // TODO handle exceptions
    use file = File.Create fullPath
    // TODO use async version?
    stream.CopyTo file
    Ok ()

let saveFileFromStream overwrite basePath fileName stream =
    // TODO handle possible exceptions
    let dirPath = FileInfo(basePath).Directory.FullName
    let path = Path.combine dirPath fileName

    match File.Exists path, overwrite with
    | true, OverwriteFile.KeepAsIs -> Error SaveFileError.FileExists
    | true, OverwriteFile.Overwrite
    | false, _ -> saveFile path stream
