namespace TubeDl.Download

open TubeDl
open TubeDl.Cli

[<RequireQualifiedAccess>]
type FinishedDlStep =
    | Metadata
    | Download
    | FileHandling
