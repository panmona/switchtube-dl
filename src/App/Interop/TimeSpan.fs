module TubeDl.TimeSpan

open System

let fromMilliseconds millis =
    float millis |> TimeSpan.FromMilliseconds

let hourMinuteSecond (span: TimeSpan) =
    span.ToString("hh\:mm\:ss")
