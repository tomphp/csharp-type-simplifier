module Web.Rendering

open Bolero
open Bolero.Html
open Parser.MessageParser
open Parser.TypeParser
open Web.Model

let private renderNamespace (description: TypeDescription) : Node =
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

let private renderTypeVars (renderType: TypeDescription -> Node) : TypeDescription -> Node =
    _.TypeVariables
    >> List.map renderType
    >> intersperse (text ", ")
    >> surround (text "<") (text ">")

let rec private renderType (model : Model) (description: TypeDescription) : Node =
    concat {
        if model.showNamespaces then
            renderNamespace description

        renderTypename description

        if description.TypeVariables.Length > 0 then
            renderTypeVars (renderType model) description
    }

let private renderText (content: string) : Node =
    span {
        attr.``class`` "message-text"
        content.Split('\n') |> List.ofArray |> List.map text |> intersperse (br {})
    }

let renderMessage model (msg: MessagePart list) =
    div {
        attr.``class`` "message"
        for part in msg do
            match part with
            | Type t -> renderType model t
            | Text t -> renderText t
            if model.addLineBreaks then
                br
    }
