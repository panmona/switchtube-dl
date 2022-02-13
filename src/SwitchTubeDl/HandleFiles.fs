namespace TubeDl

open System.IO
open FsHttp.Helper
open FsToolkit.ErrorHandling
open Microsoft.FSharpLu

open TubeDl

type FullPath = | FullPath of string

module FullPath =
    let last (FullPath path) = Text.split [| '/' |] path |> Array.last

[<RequireQualifiedAccess>]
type OverwriteFile =
    | KeepAsIs
    | Overwrite

[<RequireQualifiedAccess>]
type SaveFileError =
    | FileExists of FullPath : FullPath
    | AccessDenied
    | DirNotFound
    | IOError
    | InvalidPath of FullPath : FullPath

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

    let private saveFile fullPath (stream : Stream) =
        let (FullPath path) = fullPath
        asyncResult {
            use! file =
                try
                    File.Create path |> Ok
                with
                | :? System.UnauthorizedAccessException -> Error SaveFileError.AccessDenied
                | :? DirectoryNotFoundException -> Error SaveFileError.DirNotFound
                | :? IOException -> Error SaveFileError.IOError
                | :? System.NotSupportedException -> Error (SaveFileError.InvalidPath fullPath)

            let! _ =
                task {
                    try
                        let! res = stream.CopyToAsync file
                        return Ok res
                    with
                    | :? IOException
                    | :? System.ObjectDisposedException
                    | :? System.Threading.Tasks.TaskCanceledException -> return Error SaveFileError.IOError
                }

            return fullPath
        }

    let private saveFileFromStream overwrite fullPath stream =
        let (FullPath path) = fullPath
        async {
            match File.Exists path, overwrite with
            | true, OverwriteFile.KeepAsIs -> return Error (SaveFileError.FileExists fullPath)
            | true, OverwriteFile.Overwrite
            | false, _ -> return! saveFile fullPath stream
        }

    let fullPath basePath videoDetails videoPath =
        let fileName =
            // TODO add episode handling
            let extension = MediaType.extension videoPath.MediaType
            let episode =
                videoDetails.Episode
                |> Option.map validFileName
                |> Option.map (String.append "_")
                |> Option.defaultValue ""
            let name = validFileName videoDetails.Title
            $"%s{episode}%s{name}.%s{extension}"

        Path.combine basePath fileName |> FullPath

    let saveVideo overwrite basePath videoDetails videoPath stream =
        let path = fullPath basePath videoDetails videoPath
        saveFileFromStream overwrite path stream
