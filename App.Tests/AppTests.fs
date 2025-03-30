module AppTests

open App
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Documents
open Avalonia.Headless
open Avalonia.Headless.XUnit
open FsUnit.Xunit

type TestAppBuilder() =
    static member BuildAvaloniaApp() =
        AppBuilder.Configure<App>().UseHeadless(AvaloniaHeadlessPlatformOptions())

[<assembly: AvaloniaTestApplication(typeof<TestAppBuilder>)>]
do ()

let inlinesToText: Inline seq -> string =
    Seq.map (fun (inline_: Inline) ->
        match inline_ with
        | :? Run as run -> run.Text
        | _ -> "")
    >> Seq.toList
    >> String.concat ""

let createWindow () : Window * TextBox * TextBlock =
    let window = MainWindow()
    window.Show()
    let input = window.FindControl<TextBox> "InputTextBox"
    let output = window.FindControl<TextBlock> "OutputTextBlock"
    window, input, output

[<AvaloniaFact>]
let ``inputting a boring message`` () =
    let window, input, output = createWindow ()

    input.Focus() |> ignore
    window.KeyTextInput "boring message"

    let text = inlinesToText output.Inlines
    text |> should equal "boring message"

[<AvaloniaFact>]
let ``inputting a message with types`` () =
    let window, input, output = createWindow ()

    input.Focus() |> ignore
    window.KeyTextInput "The following is a type MyCompany.MyProject.MyType"

    let text = inlinesToText output.Inlines
    text |> should equal "The following is a type MyCompany.MyProject.MyType"

[<AvaloniaFact>]
let ``hiding namespaces`` () =
    let window, input, output = createWindow ()
    let hideNamespaces = window.FindControl<CheckBox> "HideNamespaces"

    input.Focus() |> ignore
    window.KeyTextInput "The following is a type MyCompany.MyProject.MyType"

    hideNamespaces.IsChecked <- true

    let text = inlinesToText output.Inlines
    text |> should equal "The following is a type MyType"

[<AvaloniaFact>]
let ``show only the types`` () =
    let window, input, output = createWindow ()
    let typesOnly = window.FindControl<CheckBox> "TypesOnly"

    input.Focus() |> ignore
    window.KeyTextInput "The following is a type MyCompany.MyProject.MyType"

    typesOnly.IsChecked <- true

    let text = inlinesToText output.Inlines
    text |> should equal "MyCompany.MyProject.MyType"

[<AvaloniaFact>]
let ``add line breaks`` () =
    let window, input, output = createWindow ()
    let addLineBreaks = window.FindControl<CheckBox> "AddLineBreaks"

    input.Focus() |> ignore
    window.KeyTextInput "First type is MyCompany.MyProject.MyTypeOne and second type is MyCompany.MyProject.MyTypeTwo"

    addLineBreaks.IsChecked <- true

    let text = inlinesToText output.Inlines

    text
    |> should
        equal
        ("First type is \n"
         + "MyCompany.MyProject.MyTypeOne\n"
         + " and second type is \n"
         + "MyCompany.MyProject.MyTypeTwo")
