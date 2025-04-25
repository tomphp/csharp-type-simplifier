module Web.Main

open Elmish
open Bolero
open Bolero.Html
open Parser.MessageParser
open Web.Model
open Web.Rendering

type Message =
    | ParseMessage of string
    | ShowNamespaces of bool
    | AddLineBreaks of bool
    | AddExampleMessage

let exampleMessage =
    "Unhandled exception. System.InvalidOperationException: "
    + "Unable to resolve service for type 'Example.MyClass`1+InnerOne`1[System.String,System.Int32]' "
    + "while attempting to activate 'Example.MyClass`1+InnerTwo`1[System.String,System.Int32]'."

let update message model =
    match message with
    | ParseMessage msg -> { model with Message = msg }, Cmd.none
    | ShowNamespaces show -> { model with ShowNamespaces = show }, Cmd.none
    | AddLineBreaks add -> { model with AddLineBreaks = add }, Cmd.none
    | AddExampleMessage -> { model with Message = exampleMessage }, Cmd.none

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main
        .Home()
        .ParsedMessage(model.Message, ParseMessage >> dispatch)
        .ToggleNamespaces(model.ShowNamespaces, ShowNamespaces >> dispatch)
        .AddLineBreaks(model.AddLineBreaks, AddLineBreaks >> dispatch)
        .AddExampleMessage(fun _ -> dispatch AddExampleMessage)
        .Output(
            model.Message
            |> parseMessage
            |> Result.map (renderMessage model)
            |> Result.defaultValue (text "error")
        )
        .Elt()

let view model dispatch =
    Main().Body(homePage model dispatch).Elt()

type WebApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program = Program.mkProgram (fun _ -> initModel, Cmd.none) update view
