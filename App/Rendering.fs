module App.Rendering

open App.FunctionalUtils
open Avalonia.Controls.Documents
open Avalonia.Media
open Parser

let rec buildTypeInlines (typeDescription : TypeDescription) (hideNamespaces : bool) : Run list =
    let namespaceRuns =
        if typeDescription.Namespace <> [] && not hideNamespaces then
            typeDescription.Namespace
            |> List.map (fun n -> Run(
                n,
                Foreground = SolidColorBrush Colors.BlueViolet
            ))
        else
            []

    let symbol = Run(
        typeDescription.TypeName,
        Foreground = SolidColorBrush Colors.GreenYellow
    )

    let namePart =
        [yield! namespaceRuns; yield symbol]
        |> intersperse (Run(".", Foreground = SolidColorBrush Colors.Gray))

    let typeVars =
        if typeDescription.TypeVariables <> [] then
            [
                yield Run "<"
                yield! typeDescription.TypeVariables
                    |> List.collect (fun typeVar -> [
                        yield! buildTypeInlines typeVar hideNamespaces
                        yield Run(", ", Foreground = SolidColorBrush Colors.Gray)
                    ])
                    |> dropLast
                yield Run ">"
            ]
        else
            []

    [
        yield! namePart
        yield! typeVars
    ]

let renderMessagePart typesOnly hideNamespaces part =
    match part with
    | Text(str) ->
        if not typesOnly
        then [Run(str, Foreground = SolidColorBrush Colors.Lavender)]
        else []
    | Type(desc) ->
        buildTypeInlines desc hideNamespaces

let renderMessageParts hideNamespaces addLineBreaks typesOnly parts =
    parts
    |> Seq.map (renderMessagePart typesOnly hideNamespaces)
    |> Seq.toList
    |> (fun x -> if addLineBreaks then intersperse [Run "\n"] x else x)
    |> List.concat

let renderParseError msg =
    let error = Run "Failed to parse type\n"
    error.Foreground <- SolidColorBrush(Colors.Red)
    error.FontWeight <- FontWeight.Bold
    let errMsg = Run msg
    [error; errMsg]

let renderOutput hideNamespaces addLineBreaks typesOnly parts =
    match parts with
    | Ok(parts) ->
        renderMessageParts hideNamespaces addLineBreaks typesOnly parts
    | Error(msg) ->
        renderParseError msg
