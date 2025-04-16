module Web.Model

open Bolero

type Page = | [<EndPoint "/">] Home

type Model =
    { Page: Page
      Message: string
      ShowNamespaces: bool
      AddLineBreaks: bool }

let initModel =
    { Page = Home
      Message = ""
      ShowNamespaces = true
      AddLineBreaks = false }
