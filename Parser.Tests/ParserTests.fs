module ParserTests

open FsUnit.Xunit
open Xunit

open Parser

[<Theory>]
[<InlineData("MyType")>]
[<InlineData("My_Type")>]
[<InlineData("My123Type")>]
let ``parseType given top top-level type returns type name`` str =
    parseType str
    |> should equal (Result.Ok { Namespace = []; TypeName = str; TypeVariables = [] } : Result<TypeDescription, string>)

[<Theory>]
[<InlineData("MyNamespace.MyType", "MyNamespace", "MyType")>]
[<InlineData("A.B.C.E", "A.B.C", "E")>]
let ``parseType given namespaced type returns type name`` (str, ns: string, typeName) =
    parseType str
    |> should equal (Result.Ok { Namespace = ns.Split('.') |> List.ofArray; TypeName = typeName; TypeVariables = [] } : Result<TypeDescription, string>)

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
    parseType str
    |> should equal (Result.Ok {
        Namespace = []
        TypeName = typeName
        TypeVariables = typeVars.Split(',')
                   |> List.ofArray
                   |> List.map (fun tn -> { Namespace = []; TypeName = tn; TypeVariables = [] })
    } : Result<TypeDescription, string>)

[<Theory>]
[<InlineData("A<string>", "A", "string")>]
[<InlineData("A<B>", "A", "B")>]
[<InlineData("A<Namespace.B>", "A", "Namespace.B")>]
[<InlineData("A<B,C>", "A", "B,C")>]
let ``parseType given typed generic type returns type variables`` (str, typeName, typeVars: string) =
    parseType str
    |> should equal (Result.Ok {
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
    } : Result<TypeDescription, string>)
