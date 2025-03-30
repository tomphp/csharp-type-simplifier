module App.Rendering

open Avalonia.Controls.Documents
open Avalonia.Media
open ListUtils
open Parser

type RenderOptions =
    { HideNamespaces: bool
      AddLineBreaks: bool
      TypesOnly: bool }

let private coloredText (color: Color) (text: string) : Run =
    Run(text, Foreground = SolidColorBrush color)

let private namespacePart = coloredText Colors.BlueViolet

let private typeName = coloredText Colors.GreenYellow

let private namespaceSeparator = coloredText Colors.Gray "."

let private typeVarSeparator = coloredText Colors.Gray ", "

let private namespaceParts = List.map namespacePart

let fullyQualifiedType typeDescription hideNamespaces =
    [ if not hideNamespaces then
          yield! namespaceParts typeDescription.Namespace
      yield typeName typeDescription.TypeName ]
    |> intersperse namespaceSeparator

let rec private buildTypeInlines (typeDescription: TypeDescription) (hideNamespaces: bool) : Run list =
    let typeVars =
        match typeDescription.TypeVariables with
        | [] -> []
        | vars ->
            [ yield Run "<"
              yield!
                  vars
                  |> List.map (fun var -> buildTypeInlines var hideNamespaces)
                  |> intersperse [ typeVarSeparator ]
                  |> List.concat
              yield Run ">" ]

    [ yield! fullyQualifiedType typeDescription hideNamespaces; yield! typeVars ]

let private messagePart (options: RenderOptions) part =
    match part with
    | Text str when not options.TypesOnly -> [ coloredText Colors.Lavender str ]
    | Type desc -> buildTypeInlines desc options.HideNamespaces
    | _ -> []

let messageParts (options: RenderOptions) (parts: MessagePart seq) : Run list =
    parts
    |> Seq.map (messagePart options)
    |> Seq.toList
    |> (if options.AddLineBreaks then
            intersperse [ Run "\n" ]
        else
            id)
    |> List.concat

let parseError (msg: string) : Run list =
    let error = Run "Failed to parse type\n"
    error.Foreground <- SolidColorBrush(Colors.Red)
    error.FontWeight <- FontWeight.Bold
    let errMsg = Run msg
    [ error; errMsg ]
