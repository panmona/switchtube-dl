namespace TubeDl

open System.IO
open FsHttp.Helper
open FsToolkit.ErrorHandling
open Microsoft.FSharpLu

[<RequireQualifiedAccess>]
type OverwriteFile =
    | KeepAsIs
    | Overwrite

[<RequireQualifiedAccess>]
type SaveFileError =
    | FileExists
    | AccessDenied
    | DirNotFound
    | IOError
    | InvalidPath of FullPath: string

type FullPath = | FullPath of string

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

        let isSuboptimalChar =
            function
            | '-'
            | '.' -> true
            | _ -> false

        // TODO replace - with empty
        let replaceSpace =
            function
            | ' ' -> '_'
            | c -> c

        str
        |> String.normalize
        |> String.toCharArray
        |> Array.filter (fun c ->
            Char.nonSpacingMark c
            && not (isInvalidChar c)
            && not (isSuboptimalChar c)
        )
        |> Array.map replaceSpace
        |> System.String

    let private saveFile (FullPath fullPath) (stream : Stream) =
        asyncResult {
            // TODO handle exceptions
            use! file =
                try
                    File.Create fullPath |> Ok
                with
                | :? System.UnauthorizedAccessException -> Error SaveFileError.AccessDenied
                | :? DirectoryNotFoundException -> Error SaveFileError.DirNotFound
                | :? IOException -> Error SaveFileError.IOError
                | :? System.NotSupportedException -> Error (SaveFileError.InvalidPath fullPath)

            let _ =
                stream.CopyToAsync file |> Async.AwaitTask

            return fullPath
        }

    let private saveFileFromStream overwrite (FullPath fullPath) stream =
        async {
            match File.Exists fullPath, overwrite with
            | true, OverwriteFile.KeepAsIs -> return Error SaveFileError.FileExists
            | true, OverwriteFile.Overwrite
            | false, _ -> return! saveFile (FullPath fullPath) stream
        }

    let fullPath basePath videoDetails videoPath =
        let fileName =
            // TODO add episode handling
            let extension = MediaType.extension videoPath.MediaType
            let name = validFileName videoDetails.Title
            $"%s{name}.%s{extension}"
        // TODO handle possible exceptions

        Path.combine basePath fileName |> FullPath

    let saveVideo overwrite basePath videoDetails videoPath stream =
        let path = fullPath basePath videoDetails videoPath
        saveFileFromStream overwrite path stream
