module Web.Main

open Elmish
open Bolero
open Bolero.Html
open Parser.MessageParser
open Web.Model
open Web.Rendering

type Message =
    | SetPage of Page
    | ParseMessage of string
    | ShowNamespaces of bool
    | AddLineBreaks of bool

let update message model =
    match message with
    | SetPage page -> { model with Page = page }, Cmd.none
    | ParseMessage msg -> { model with Message = msg }, Cmd.none
    | ShowNamespaces show -> { model with ShowNamespaces = show }, Cmd.none
    | AddLineBreaks add -> { model with AddLineBreaks = add }, Cmd.none

let router = Router.infer SetPage _.Page

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main
        .Home()
        .ParsedMessage(model.Message, ParseMessage >> dispatch)
        .ToggleNamespaces(model.ShowNamespaces, ShowNamespaces >> dispatch)
        .AddLineBreaks(model.AddLineBreaks, AddLineBreaks >> dispatch)
        .Output(
            model.Message
            |> parseMessage
            |> Result.map (renderMessage model)
            |> Result.defaultValue (text "error")
        )
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem().Active(if model.Page = page then "is-active" else "").Url(router.Link page).Text(text).Elt()

let view model dispatch =
    Main()
        .Menu(concat { menuItem model Home "Home" })
        .Body(
            cond model.Page
            <| function
                | Home -> homePage model dispatch
        )
        .Elt()

type WebApp() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.WebApp

    override this.Program =
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
