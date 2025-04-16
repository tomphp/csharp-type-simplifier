module Web.Model

open Bolero

type Page = | [<EndPoint "/">] Home

type Model =
    { page: Page
      message: string
      showNamespaces: bool
      addLineBreaks: bool }

let initModel =
    { page = Home
      message = ""
      showNamespaces = true
      addLineBreaks = false }
