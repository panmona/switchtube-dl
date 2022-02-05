module TubeDl.Uri

open System

let initAbsolute uriStr = Uri uriStr

let initRelative (baseUri : string) (uriStr : string) =
    Uri (initAbsolute baseUri, uriStr)

let absoluteUri (uri : Uri) = uri.AbsoluteUri
