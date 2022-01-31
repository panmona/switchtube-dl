// For more information see https://aka.ms/fsharp-console-apps

open System.Net.Http.Headers
open System.Net

open Microsoft.FSharpLu.Text

open FsHttp
open FsHttp.DslCE
open FSharp.Data

type ErrorTypes =
    /// Token is incorrect
    | UnauthorizedAccess
    /// This resource was not found
    | ResourceNotFound
    /// An unexpected api error occured
    | ApiError

// TODO: context or something or always token?
let requestChannelVideos token channelId =
    http {
        // GET "https://tube.switch.ch/api/v1/browse/channels"
        GET $"https://tube.switch.ch/api/v1/browse/channels/%s{channelId}/videos"
        CacheControl "no-cache"
        Authorization $"Token %s{token}"
    }

let tryParseLinkHeader (header : string) =
    // For example: "<https://tube.switch.ch/api/v1/browse/channels?page=3>; rel=\"next\""
    let nextPage =
        split [| ';' |] header
        |> Array.head
        |> trim [| '<'; '>' |]

    let wellFormed =
        System.Uri.IsWellFormedUriString (nextPage, System.UriKind.Absolute)

    if wellFormed then
        Some nextPage
    else
        None

let nextPage (headers : HttpResponseHeaders) =
    let headerOpt =
        if headers.Contains HttpResponseHeaders.Link then
            headers.GetValues HttpResponseHeaders.Link
            |> Seq.tryHead
        else
            None

    let hasNextPage = endWith "rel=\"next\""

    match headerOpt with
    | Some header when hasNextPage header -> tryParseLinkHeader header
    | Some _
    | None -> None

let request =
    let resp = requestChannelVideos "token" "FMnsK03wWD"
    // TODO: improve return value
    // TODO: stream?
    match resp.statusCode with
    | HttpStatusCode.OK ->
        (Response.toJson resp, nextPage resp.headers)
        |> Ok
    | HttpStatusCode.Unauthorized -> Error UnauthorizedAccess
    | HttpStatusCode.NotFound -> Error ResourceNotFound
    | HttpStatusCode.InternalServerError
    | _ -> Error ApiError

match request with
| Ok (json, header) ->
    printfn "%A" json
    printfn "%A" header
| Error errorValue -> printfn "%A" errorValue
