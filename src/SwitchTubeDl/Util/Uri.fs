module TubeDl.Uri

open System

let initAbsolute uriStr = Uri uriStr

let initRelative (baseUri : string) (uriStr : string) = Uri (initAbsolute baseUri, uriStr)

let absoluteUri (uri : Uri) = uri.AbsoluteUri

let wellFormedAbsoluteUri uri =
    match Uri.IsWellFormedUriString (uri, UriKind.Absolute) with
    | true -> initAbsolute uri |> Some
    | false -> None
