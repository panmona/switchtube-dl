[<AutoOpen>]
module TubeDl.Int

let (|Int|_|) str =
    match System.Int32.TryParse (str : string) with
    | true, int -> Some (int)
    | _ -> None
