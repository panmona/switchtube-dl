[<RequireQualifiedAccess>]
module TubeDl.TimeSpan

open System
open Microsoft.FSharp.Core

let fromMilliseconds millis =
    float millis |> TimeSpan.FromMilliseconds

let hourMinuteSecond (span : TimeSpan) = span.ToString ("hh\:mm\:ss")
