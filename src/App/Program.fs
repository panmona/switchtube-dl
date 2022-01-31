module TubeDl.Program

open System.Net.Http.Headers
open System.Net

open Microsoft.FSharpLu.Text

open FsHttp
open FsHttp.DslCE
open FSharp.Data
open Thoth.Json.Net

type ErrorTypes =
    /// Token is incorrect
    | UnauthorizedAccess
    /// This resource was not found
    | ResourceNotFound
    /// Be gentle with the api and send less requests!
    | TooManyRequests
    /// An unexpected api error occured
    | ApiError

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
    let api = ApiRequests.api ApiRequests.RequestType.ChannelVideos
    let resp = api "token" "FMnsK03wWD"
    // TODO: improve return value
    // TODO: stream?
    match resp.statusCode with
    | HttpStatusCode.OK ->
        (Response.toText resp, nextPage resp.headers)
        |> Ok
    | HttpStatusCode.Unauthorized
    | HttpStatusCode.Forbidden -> Error UnauthorizedAccess
    | HttpStatusCode.NotFound -> Error ResourceNotFound
    | HttpStatusCode.TooManyRequests -> Error TooManyRequests
    | HttpStatusCode.InternalServerError
    | _ -> Error ApiError

match request with
| Ok (json, header) ->
    printfn "%A" header
    let x = Decode.fromString (Decode.list ChannelDetails.decoder) json
    printfn "%A" x
| Error errorValue -> printfn "%A" errorValue
