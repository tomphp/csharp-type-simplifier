namespace App

open App.FunctionalUtils
open App.Rendering
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Documents
open Avalonia.Markup.Xaml
open Avalonia.Media

open Parser

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
            let text = inputTextBox.Text |> Option.ofObj |> Option.defaultValue ""
            let parts = parseMessage text

            let runs =
                renderOutput
                    hideNamespaces.IsChecked.Value
                    addLineBreaks.IsChecked.Value
                    typesOnly.IsChecked.Value
                    parts

            let inlines = InlineCollection()

            for run in runs do
                inlines.Add run

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
