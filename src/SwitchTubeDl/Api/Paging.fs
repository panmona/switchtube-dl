module TubeDl.Paging

open Microsoft.FSharpLu

open TubeDl

let tryParseLinkHeader (header : string) =
    // For example: "<https://tube.switch.ch/api/v1/browse/channels?page=3>; rel=\"next\""
    let nextPage =
        Text.split [| ';' |] header |> Array.head |> Text.trim [| '<' ; '>' |]

    Uri.wellFormedAbsoluteUri nextPage |> Option.map (fun u -> u.AbsoluteUri)

let tryGetNextPageUri (headers : System.Net.Http.Headers.HttpResponseHeaders) =
    let headerOpt =
        let linkHeader = "Link"

        match headers.TryGetValues linkHeader with
        | true, value -> value |> Seq.head |> Some
        | _ -> None

    let hasNextPage = Text.endWith "rel=\"next\""

    match headerOpt with
    | Some header when hasNextPage header -> tryParseLinkHeader header
    | Some _
    | None -> None
