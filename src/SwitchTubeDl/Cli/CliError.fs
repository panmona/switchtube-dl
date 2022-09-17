namespace TubeDl.Cli

open TubeDl
open TubeDl.Rich

[<RequireQualifiedAccess>]
type DownloadError =
    | TubeInfoError of TubeInfoError
    | SaveFileError of SaveFileError

[<RequireQualifiedAccess>]
module DownloadError =
    let errorMsg (cfg : CompleteCfg) (error : DownloadError) =
        let apiErrorMsg =
            function
            | ApiError.UnauthorizedAccess -> "A valid token needs to be provided."
            | ApiError.ResourceNotFound -> "An existing id needs to be provided."
            | ApiError.ApiError
            | ApiError.TooManyRequests -> "SwitchTube encountered an error, please try again later."

        let fileErrorMsg =
            function
            | SaveFileError.AccessDenied ->
                $"Wasn't able to write a file to the path [italic]%s{esc cfg.Path}[/]. Please ensure that the path is writable."
            | SaveFileError.FileExists fullPath ->
                let fileName = FullPath.last fullPath
                $"The video %s{esc fileName} exists already in the path [italic]%s{esc cfg.Path}[/]. If you want to overwrite it, use the option [bold yellow]-f[/]."
            | SaveFileError.InvalidPath (FullPath path) ->
                $"Wasn't able to save the video to the following invalid path: [italic]%s{esc path}[/]."
                + $"If the name of the video file seems to be the cause: %s{esc GitHub.createIssue}."
            | SaveFileError.DirNotFound -> "The given path was invalid "
            | SaveFileError.IOError ->
                "There was an IO error while saving the file. Please check your path and try again."

        match error with
        | DownloadError.TubeInfoError (TubeInfoError.ApiError apiError) -> apiErrorMsg apiError
        | DownloadError.TubeInfoError (TubeInfoError.DecodeError s) ->
            "There was an error while decoding the JSON received from the API.\n"
            + $"%s{GitHub.createIssue} that also contains the following info:\n\n"
            + $"[italic]%s{esc s}[/]"
        | DownloadError.SaveFileError fileError -> fileErrorMsg fileError

[<RequireQualifiedAccess>]
type CliError =
    | ArgumentsNotSpecified
    | DownloadError of DownloadError

module CliError =
    let getExitCode result =
        match result with
        | Ok () -> 0
        | Error err ->

        match err with
        | CliError.ArgumentsNotSpecified -> 1
        | CliError.DownloadError dlErr ->

        match dlErr with
        | DownloadError.TubeInfoError (TubeInfoError.ApiError apiErr) ->
            match apiErr with
            | ApiError.UnauthorizedAccess -> 2
            | ApiError.ResourceNotFound -> 3
            | ApiError.TooManyRequests -> 5
            | ApiError.ApiError -> 6
        | DownloadError.TubeInfoError (TubeInfoError.DecodeError _) -> 7
        | DownloadError.SaveFileError _ -> 8
