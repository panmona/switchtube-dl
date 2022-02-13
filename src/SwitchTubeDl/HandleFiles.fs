namespace TubeDl

open System.IO
open FsHttp.Helper
open FsToolkit.ErrorHandling
open Microsoft.FSharpLu

open TubeDl

type FullPath = | FullPath of string

module FullPath =
    let last (FullPath path) = Text.split [| Path.directorySeparator |] path |> Array.last

[<RequireQualifiedAccess>]
type ExistingFilesHandling =
    | KeepAsIs
    | Skip
    | Overwrite

[<RequireQualifiedAccess>]
type SaveFileError =
    | FileExists of FullPath : FullPath
    | AccessDenied
    | DirNotFound
    | IOError
    | InvalidPath of FullPath : FullPath

[<RequireQualifiedAccess>]
type FileWriteResult =
    | Written of FullPath
    | Skipped

module HandleFiles =
    let private replaceInvalidChars str =
        // This list may be incomplete
        let isInvalid c =
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

        str
        |> String.toCharArray
        |> Array.filter (isInvalid >> not)
        |> System.String

    let private validFileName str =
        let isSuboptimalChar =
            function
            | '-'
            | '.' -> true
            | _ -> false

        let replaceSpaces = Regex.replace "\\s+" "_" // https://stackoverflow.com/a/2878200

        str
        |> replaceInvalidChars
        |> String.normalize
        |> String.toCharArray
        |> Array.filter (fun c -> Char.nonSpacingMark c && not (isSuboptimalChar c))
        |> System.String
        |> replaceSpaces

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
            | true, ExistingFilesHandling.KeepAsIs -> return Error (SaveFileError.FileExists fullPath)
            | true, ExistingFilesHandling.Skip -> return Ok FileWriteResult.Skipped
            | true, ExistingFilesHandling.Overwrite
            | false, _ ->
                return!
                    saveFile fullPath stream
                    |> AsyncResult.map FileWriteResult.Written
        }

    let fileName videoDetails videoPath =
        let extension = MediaType.extension videoPath.MediaType

        let episode =
            videoDetails.Episode
            |> Option.map validFileName
            |> Option.map (String.append "_")
            |> Option.defaultValue ""

        let name = validFileName videoDetails.Title
        $"%s{episode}%s{name}_%s{videoDetails.Id}.%s{extension}"

    let fullPath basePath file = Path.combine basePath file |> FullPath

    let saveVideo overwrite basePath videoDetails videoPath stream =
        let path =
            fileName videoDetails videoPath
            |> fullPath basePath

        saveFileFromStream overwrite path stream
