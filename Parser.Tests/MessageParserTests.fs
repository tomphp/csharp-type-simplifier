module Parser.Tests.MessageParserTests

open FsUnit.Xunit
open Parser.MessageParser
open Xunit

[<Fact>]
let ``parseMessage given only message returns message`` () =
    let actual = parseMessage "no types here"
    let expected: Result<MessagePart list, string> = Ok [ Text "no types here" ]
    actual |> should equal expected

[<Fact>]
let ``parseMessage given only type returns type`` () =
    let actual = parseMessage "Alpha.Beta"

    let expected: Result<MessagePart list, string> =
        Ok
            [ Type
                  { Namespace = [ "Alpha" ]
                    TypeName = "Beta"
                    TypeVariables = [] } ]

    actual |> should equal expected

[<Fact>]
let ``parseMessage full message`` () =
    let actual = parseMessage "Type Alpha.Beta does not match type 'Beta<string>'."

    let expected: Result<MessagePart list, string> =
        Ok
            [ Text "Type "
              Type
                  { Namespace = [ "Alpha" ]
                    TypeName = "Beta"
                    TypeVariables = [] }
              Text " does not match type '"
              Type
                  { Namespace = []
                    TypeName = "Beta"
                    TypeVariables =
                      [ { Namespace = []
                          TypeName = "string"
                          TypeVariables = [] } ] }
              Text "'." ]

    actual |> should equal expected
