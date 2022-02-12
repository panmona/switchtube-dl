namespace TubeDl.Cli

open TubeDl

type DownloadError =
    | TubeInfoError of TubeInfoError
    | SaveFileError of SaveFileError

module DownloadError =
    let errorMsg (cfg : CliCfg) (error : DownloadError) =
        let apiErrorMsg =
            function
            | Api.UnauthorizedAccess -> "A valid token needs to be provided."
            | Api.ResourceNotFound -> "An existing id needs to be provided."
            | Api.ApiError
            | Api.TooManyRequests -> "SwitchTube encountered an error, please try again later."

        let fileErrorMsg =
            function
            | SaveFileError.AccessDenied ->
                $"Wasn't able to write a file to the path [italic]%s{cfg.Path}[/]. Please ensure that the path is writable."
            | SaveFileError.FileExists fullPath ->
                let fileName = FullPath.last fullPath
                $"The video %s{fileName} exists already in the path [italic]%s{cfg.Path}[/]. If you want to overwrite it, use the option [bold yellow]-f[/]."
            | SaveFileError.InvalidPath (FullPath path) ->
                $"Wasn't able to save the video to the following invalid path: [italic]%s{path}[/]."
                + $"If the name of the video file seems to be the cause: %s{GitHub.createIssue}."
            | SaveFileError.DirNotFound -> "The given path was invalid "
            | SaveFileError.IOError ->
                "There was an IO error while saving the file. Please check your path and try again."

        match error with
        | TubeInfoError (TubeInfoError.ApiError apiError) -> apiErrorMsg apiError
        | TubeInfoError (TubeInfoError.DecodeError s) ->
            "There was an error while decoding the JSON received from the API.\n"
            + $"%s{GitHub.createIssue} that also contains the following info:\n\n"
            + $"[italic]%s{s}[/]"
        | SaveFileError fileError -> fileErrorMsg fileError

type CliError =
    | ArgumentsNotSpecified
    | DownloadError of DownloadError

module CliError =
    let getExitCode result =
        match result with
        | Ok () -> 0
        | Error err ->

        match err with
        | ArgumentsNotSpecified -> 1
        | DownloadError dlErr ->

        match dlErr with
        | TubeInfoError (TubeInfoError.ApiError _) -> 2
        | TubeInfoError (TubeInfoError.DecodeError _) -> 3
        | SaveFileError _ -> 4
