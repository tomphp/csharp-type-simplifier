namespace App

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Documents
open Avalonia.Markup.Xaml

open Parser
open Avalonia.Media

type MainWindow () as this =
    inherit Window ()

    let mutable inputTextBox: TextBox = null
    let mutable outputTextBlock: TextBlock = null
    let mutable hideNamespaces: CheckBox = null

    do this.InitializeComponent()

    member private this.InitializeComponent() =
        let intersperse separator xs = [
            if List.isEmpty xs then ()
            else
                yield xs[0]
                for x in xs[1..] do
                    yield separator
                    yield x
        ]

        let dropLast (xs : 'T list) = xs[..xs.Length-2]

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

        let updateOutput () =
            let text = inputTextBox.Text
            let description = parseType text

            let inlines = InlineCollection()
            match description with
            | Ok(desc) ->
                for i in buildTypeInlines desc hideNamespaces.IsChecked.Value do
                    inlines.Add(i)
            | Error(msg) ->
                let error = Run "Failed to parse type\n"
                error.Foreground <- SolidColorBrush(Colors.Red)
                error.FontWeight <- FontWeight.Bold
                let errMsg = Run msg
                inlines.Add(error)
                inlines.Add(errMsg)

            outputTextBlock.Inlines <- inlines

#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)

        inputTextBox <- this.FindControl<TextBox>("InputTextBox")
        outputTextBlock <- this.FindControl<TextBlock>("OutputTextBlock")
        hideNamespaces <- this.FindControl<CheckBox>("HideNamespaces")

        inputTextBox.TextChanged.Add(fun _ -> updateOutput())
        hideNamespaces.IsCheckedChanged.Add(fun _ -> updateOutput())

