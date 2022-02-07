namespace TubeDl

open System.IO
open FsHttp.Helper
open Microsoft.FSharpLu

[<RequireQualifiedAccess>]
type OverwriteFile =
    | KeepAsIs
    | Overwrite

[<RequireQualifiedAccess>]
type SaveFileError = | FileExists

module HandleFiles =
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

        // TODO replace - with empty
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
        async {
            // TODO handle exceptions
            use file = File.Create fullPath
            // TODO use async version?
            do! stream.CopyToAsync file |> Async.AwaitTask
            return Ok ()
        }

    let private saveFileFromStream overwrite fullPath stream =
        async {
            match File.Exists fullPath, overwrite with
            | true, OverwriteFile.KeepAsIs -> return Error SaveFileError.FileExists
            | true, OverwriteFile.Overwrite
            | false, _ -> return! saveFile fullPath stream
        }

    let saveVideo overwrite basePath videoDetails videoPath stream =
        let fileName =
            // TODO add episode handling
            let extension = MediaType.extension videoPath.MediaType
            let name = validFileName videoDetails.Title
            $"%s{name}.%s{extension}"

        let path =
            // TODO handle possible exceptions

            // TODO this doesn't fully work yet? it seems to always remove one level!
            let dirPath = FileInfo(basePath).Directory.FullName
            Path.combine dirPath fileName

        saveFileFromStream overwrite path stream
