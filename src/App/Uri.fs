module TubeDl.Uri

let initAbsolute uriStr = System.Uri uriStr

let initRelative (baseUri : string) (uriStr : string) =
    System.Uri (initAbsolute baseUri, uriStr)

let absoluteUri (uri : System.Uri) = uri.AbsoluteUri
