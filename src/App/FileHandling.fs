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

let private validFileName str =
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

    str
    |> String.normalize
    |> String.toCharArray
    |> Array.filter (fun c -> Char.nonSpacingMark c && isInvalidChar c |> not)
    |> Array.map replaceSpace
    |> System.String

let private saveFile fullPath (stream : Stream) =
    // TODO handle exceptions
    use file = File.Create fullPath
    // TODO use async version?
    stream.CopyTo file
    Ok ()


let private saveFileFromStream overwrite fullPath stream =
    match File.Exists fullPath, overwrite with
    | true, OverwriteFile.KeepAsIs -> Error SaveFileError.FileExists
    | true, OverwriteFile.Overwrite
    | false, _ -> saveFile fullPath stream

let saveVideo overwrite basePath videoDetails videoPath stream =
    let fileName =
        let extension = MediaType.extension videoPath.MediaType
        let name = validFileName videoDetails.Title
        $"%s{name}.%s{extension}"

    let path =
        // TODO? Microsoft.FSharpLu.File.createDirIfNotExists. but probably ask user first!
        // TODO handle possible exceptions
        let dirPath = FileInfo(basePath).Directory.FullName
        Path.combine dirPath fileName

    saveFileFromStream overwrite path stream
