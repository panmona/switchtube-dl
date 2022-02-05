module TubeDl.TimeSpan

let fromMilliseconds millis =
    float millis |> System.TimeSpan.FromMilliseconds

// TODO functions for human readable
