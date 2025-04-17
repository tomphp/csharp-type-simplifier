module Web.Model

type Model =
    { Message: string
      ShowNamespaces: bool
      AddLineBreaks: bool }

let initModel =
    { Message = ""
      ShowNamespaces = true
      AddLineBreaks = false }
