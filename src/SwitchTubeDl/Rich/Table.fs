[<RequireQualifiedAccess>]
module TubeDl.Rich.Table

open Spectre.Console

[<RequireQualifiedAccess>]
type Alignment =
    | Default
    | Center
    | Right

type Column = {
    Title : string
    Alignment : Alignment
}

[<RequireQualifiedAccess>]
module Column =
    let init title = {
        Title = title
        Alignment = Alignment.Default
    }

    let withAlign title align = { Title = title ; Alignment = align }

let setBorder table border =
    HasTableBorderExtensions.Border (table, border) |> ignore

let addColumns (table : Table) columns =
    let addColumn col =
        let tableCol = TableColumn col.Title

        match col.Alignment with
        | Alignment.Default -> ()
        | Alignment.Right -> tableCol.RightAligned () |> ignore
        | Alignment.Center -> tableCol.Centered () |> ignore

        table.AddColumn tableCol |> ignore

    columns |> List.iter addColumn

let addRows (table : Table) rows =
    let addRows (row : string list) =
        table.AddRow (columns = List.toArray row)

    List.toArray rows |> Array.map addRows |> ignore

let table border columns rows =
    let table = Table ()
    setBorder table border
    addColumns table columns
    addRows table rows
    AnsiConsole.Write table
