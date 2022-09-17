namespace TubeDl

type Token = | Token of string

[<RequireQualifiedAccess>]
type RequestType =
    | ChannelDetails
    | VideoDetails
    | VideoPaths
    | DownloadVideo

[<RequireQualifiedAccess>]
type ApiError =
    /// Token is incorrect
    | UnauthorizedAccess
    /// This resource was not found
    | ResourceNotFound
    /// Be gentle with the api and send less requests!
    | TooManyRequests
    /// An unexpected api error occured
    | ApiError
