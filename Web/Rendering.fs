module Web.Rendering

open Bolero
open Bolero.Html
open Parser.MessageParser
open Parser.TypeParser
open Web.Model

let private renderNamespace (description: FullyQualifiedTypeDescription) : Node =
    concat {
        for n in description.Namespace do
            span {
                attr.``class`` "message-namespace"
                n
            }

            text "."
    }

let private renderTypename (description: TypeDescription) =
    span {
        attr.``class`` "message-typename"
        text description.TypeName
    }

let private intersperse (separator: Node) (nodes: Node list) =
    concat {
        if nodes.Length > 0 then
            nodes[0]

        for node in nodes[1..] do
            separator
            node
    }

let private surround (left: Node) (right: Node) (content: Node) =
    concat {
        left
        content
        right
    }

let private renderTypeVars (renderType: FullyQualifiedTypeDescription -> Node) : TypeDescription -> Node =
    _.TypeVariables
    >> List.map renderType
    >> intersperse (text ", ")
    >> surround (text "<") (text ">")

let private renderType (renderType: FullyQualifiedTypeDescription -> Node) (description: TypeDescription) : Node =
    concat {
        renderTypename description

        if description.TypeVariables.Length > 0 then
            renderTypeVars renderType description
    }

// fsharplint:disable-next-line UnneededRecKeyword
let rec private renderFullyQualifiedType (model: Model) (description: FullyQualifiedTypeDescription) : Node =
    concat {
        if model.ShowNamespaces then
            renderNamespace description

        intersperse
            (text ".")
            (description.TypeDescription
             |> List.map (renderType (renderFullyQualifiedType model)))
    }

let private renderText (content: string) : Node =
    span {
        attr.``class`` "message-text"
        content.Split('\n') |> List.ofArray |> List.map text |> intersperse (br { })
    }

let renderMessage model (msg: MessagePart list) =
    div {
        attr.``class`` "message"

        for part in msg do
            match part with
            | Type t -> renderFullyQualifiedType model t
            | Text t -> renderText t

            if model.AddLineBreaks then
                br
    }
