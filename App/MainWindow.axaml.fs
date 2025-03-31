namespace App

open App.Rendering
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Documents
open Avalonia.Markup.Xaml

open Parser.MessageParser

type MainWindow() as this =
    inherit Window()

    let mutable inputTextBox: TextBox = null
    let mutable outputTextBlock: TextBlock = null
    let mutable hideNamespaces: CheckBox = null
    let mutable addLineBreaks: CheckBox = null
    let mutable typesOnly: CheckBox = null

    do this.InitializeComponent()

    member private this.InitializeComponent() =
        let updateOutput () =
            let options =
                { HideNamespaces = hideNamespaces.IsChecked.GetValueOrDefault false
                  AddLineBreaks = addLineBreaks.IsChecked.GetValueOrDefault false
                  TypesOnly = typesOnly.IsChecked.GetValueOrDefault false }

            let text = inputTextBox.Text |> Option.ofObj |> Option.defaultValue ""
            let parts = parseMessage text

            let runs =
                parts
                |> function
                    | Ok(parts) -> messageParts options parts
                    | Error(msg) -> parseError msg

            let inlines = InlineCollection()
            runs |> List.iter inlines.Add
            outputTextBlock.Inlines <- inlines

#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)

        inputTextBox <- this.FindControl<TextBox>("InputTextBox")
        outputTextBlock <- this.FindControl<TextBlock>("OutputTextBlock")
        hideNamespaces <- this.FindControl<CheckBox>("HideNamespaces")
        addLineBreaks <- this.FindControl<CheckBox>("AddLineBreaks")
        typesOnly <- this.FindControl<CheckBox>("TypesOnly")

        inputTextBox.TextChanged.Add(fun _ -> updateOutput ())
        hideNamespaces.IsCheckedChanged.Add(fun _ -> updateOutput ())
        addLineBreaks.IsCheckedChanged.Add(fun _ -> updateOutput ())
        typesOnly.IsCheckedChanged.Add(fun _ -> updateOutput ())
