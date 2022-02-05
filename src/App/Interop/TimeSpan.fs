module TubeDl.TimeSpan

open System

let fromMilliseconds millis =
    float millis |> TimeSpan.FromMilliseconds

// TODO functions for human readable
