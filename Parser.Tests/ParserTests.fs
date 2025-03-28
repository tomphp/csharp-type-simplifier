module ParserTests

open FsUnit.Xunit
open Xunit

open Parser

[<Theory>]
[<InlineData("MyType")>]
[<InlineData("My_Type")>]
[<InlineData("My123Type")>]
let ``parseType given top top-level type returns type name`` str =
    let actual = parseType str
    let expected : Result<TypeDescription, string> =
        Result.Ok { Namespace = []; TypeName = str; TypeVariables = [] }
    actual |> should equal expected

[<Theory>]
[<InlineData("MyNamespace.MyType", "MyNamespace", "MyType")>]
[<InlineData("A.B.C.E", "A.B.C", "E")>]
let ``parseType given namespaced type returns type name`` (str, ns: string, typeName) =
    let actual = parseType str
    let expected : Result<TypeDescription, string> =
        Result.Ok { Namespace = ns.Split('.') |> List.ofArray; TypeName = typeName; TypeVariables = [] }
    actual |> should equal expected

[<Theory>]
[<InlineData("1Type")>]
[<InlineData("_Type")>]
[<InlineData("T[ype")>]
let ``parseType given invalid typename type returns error`` str =
    Result.isError (parseType str) |> should be True

[<Theory>]
[<InlineData("OneType`1", "OneType", "T1")>]
[<InlineData("TwoType`2", "TwoType", "T1,T2")>]
let ``parseType given numbered generic type returns Tn type variables`` (str, typeName, typeVars: string) =
    let actual = parseType str
    let expected :  Result<TypeDescription, string> =
        Result.Ok {
            Namespace = []
            TypeName = typeName
            TypeVariables = typeVars.Split(',')
              |> List.ofArray
              |> List.map (fun tn -> { Namespace = []; TypeName = tn; TypeVariables = [] })
            }
    actual |> should equal expected

[<Theory>]
[<InlineData("A`1[string]", "A", "string")>]
[<InlineData("A`1[B]", "A", "B")>]
[<InlineData("A`1[Namespace.B]", "A", "Namespace.B")>]
[<InlineData("A`2[B,C]", "A", "B,C")>]
[<InlineData("A`2[B , C]", "A", "B,C")>]
let ``parseType given numbered & typed generic type returns type variables`` (str, typeName, typeVars: string) =
    let actual = parseType str
    let expected : Result<TypeDescription, string> =
        Result.Ok {
            Namespace = []
            TypeName = typeName
            TypeVariables = typeVars.Split(',')
              |> List.ofArray
              |> List.map (fun subType ->
                  let parts = subType.Split('.')
                  let subNamespace = parts[..parts.Length-2] |> List.ofArray
                  let subTypeName = parts[parts.Length-1]
                  { Namespace = subNamespace; TypeName = subTypeName; TypeVariables = [] }
               )
            }
    actual |> should equal expected

[<Theory>]
[<InlineData("A<string>", "A", "string")>]
[<InlineData("A<B>", "A", "B")>]
[<InlineData("A<Namespace.B>", "A", "Namespace.B")>]
[<InlineData("A<B,C>", "A", "B,C")>]
[<InlineData("A<B , C>", "A", "B,C")>]
let ``parseType given typed generic type returns type variables`` (str, typeName, typeVars: string) =
    let actual = parseType str
    let expected : Result<TypeDescription, string> =
        Result.Ok {
            Namespace = []
            TypeName = typeName
            TypeVariables = typeVars.Split(',')
              |> List.ofArray
              |> List.map (fun subType ->
                  let parts = subType.Split('.')
                  let subNamespace = parts[..parts.Length-2] |> List.ofArray
                  let subTypeName = parts[parts.Length-1]
                  { Namespace = subNamespace; TypeName = subTypeName; TypeVariables = [] }
               )
            }
    actual |> should equal expected

[<Fact>]
let ``parseMessage given only message returns message`` () =
    let actual = parseMessage "no types here"
    let expected : Result<MessagePart list, string> = Ok [Text "no types here"]
    actual |> should equal expected

[<Fact>]
let ``parseMessage given only type returns type`` () =
    let actual = parseMessage "Alpha.Beta"
    let expected : Result<MessagePart list, string> =
        Ok [Type {
            Namespace = ["Alpha"]
            TypeName = "Beta"
            TypeVariables = []
            }
        ]
    actual |> should equal expected

[<Fact>]
let ``parseMessage full message`` () =
    let actual = parseMessage "Type Alpha.Beta does not match type 'Beta<string>'."
    let expected : Result<MessagePart list, string> =
        Ok [
            Text "Type "
            Type {
                Namespace = ["Alpha"]
                TypeName = "Beta"
                TypeVariables = []
            }
            Text " does not match type '"
            Type {
                Namespace = []
                TypeName = "Beta"
                TypeVariables = [{Namespace = []; TypeName = "string"; TypeVariables = []}]
            }
            Text "'."
        ]
    actual |> should equal expected
