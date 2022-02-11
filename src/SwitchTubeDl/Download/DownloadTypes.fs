namespace TubeDl.Download

open TubeDl

[<RequireQualifiedAccess>]
type FinishedDlStep =
    | Metadata
    | Download
    | FileHandling of FullPath
