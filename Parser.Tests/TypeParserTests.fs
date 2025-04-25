module TypeParserTests

open FsUnit.Xunit
open Xunit

open Parser.TypeParser

module Helpers =
    open FParsec

    let parseType (str: string) : Result<FullyQualifiedTypeDescription, string> =
        match run fullyQualifiedTypeDescription str with
        | Success(result, _, _) -> Result.Ok result
        | Failure(err, _, _) -> Result.Error err

open Helpers

[<Theory>]
[<InlineData("MyType")>]
[<InlineData("My_Type")>]
[<InlineData("My123Type")>]
let ``parseType given top top-level type returns type name`` str =
    let actual = parseType str

    let expected: Result<FullyQualifiedTypeDescription, string> =
        Result.Ok
            { Namespace = []
              TypeDescription = [ { TypeName = str; TypeVariables = [] } ] }

    actual |> should equal expected

[<Theory>]
[<InlineData("MyNamespace.MyType", "MyNamespace", "MyType")>]
[<InlineData("A.B.C.E", "A.B.C", "E")>]
let ``parseType given namespaced type returns type name`` (str, ns: string, typeName) =
    let actual = parseType str

    let expected: Result<FullyQualifiedTypeDescription, string> =
        Result.Ok
            { Namespace = ns.Split('.') |> List.ofArray
              TypeDescription =
                [ { TypeName = typeName
                    TypeVariables = [] } ] }

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

    let expected: Result<FullyQualifiedTypeDescription, string> =
        Result.Ok
            { Namespace = []
              TypeDescription =
                [ { TypeName = typeName
                    TypeVariables =
                      typeVars.Split(',')
                      |> List.ofArray
                      |> List.map (fun tn ->
                          { Namespace = []
                            TypeDescription = [ { TypeName = tn; TypeVariables = [] } ] }) } ] }

    actual |> should equal expected

[<Theory>]
[<InlineData("A`1[string]", "A", "string")>]
[<InlineData("A`1[B]", "A", "B")>]
[<InlineData("A`1[Namespace.B]", "A", "Namespace.B")>]
[<InlineData("A`2[B,C]", "A", "B,C")>]
[<InlineData("A`2[B , C]", "A", "B,C")>]
let ``parseType given numbered & typed generic type returns type variables`` (str, typeName, typeVars: string) =
    let actual = parseType str

    let expected: Result<FullyQualifiedTypeDescription, string> =
        Result.Ok
            { Namespace = []
              TypeDescription =
                [ { TypeName = typeName
                    TypeVariables =
                      typeVars.Split(',')
                      |> List.ofArray
                      |> List.map (fun subType ->
                          let parts = subType.Split('.')
                          let subNamespace = parts[.. parts.Length - 2] |> List.ofArray
                          let subTypeName = parts[parts.Length - 1]

                          { Namespace = subNamespace
                            TypeDescription =
                              [ { TypeName = subTypeName
                                  TypeVariables = [] } ] }) } ] }

    actual |> should equal expected

[<Theory>]
[<InlineData("A<string>", "A", "string")>]
[<InlineData("A<B>", "A", "B")>]
[<InlineData("A<Namespace.B>", "A", "Namespace.B")>]
[<InlineData("A<B,C>", "A", "B,C")>]
[<InlineData("A<B , C>", "A", "B,C")>]
let ``parseType given typed generic type returns type variables`` (str, typeName, typeVars: string) =
    let actual = parseType str

    let expected: Result<FullyQualifiedTypeDescription, string> =
        Result.Ok
            { Namespace = []
              TypeDescription =
                [ { TypeName = typeName
                    TypeVariables =
                      typeVars.Split(',')
                      |> List.ofArray
                      |> List.map (fun subType ->
                          let parts = subType.Split('.')
                          let subNamespace = parts[.. parts.Length - 2] |> List.ofArray
                          let subTypeName = parts[parts.Length - 1]

                          { Namespace = subNamespace
                            TypeDescription =
                              [ { TypeName = subTypeName
                                  TypeVariables = [] } ] }) } ] }

    actual |> should equal expected

[<Theory>]
[<InlineData("A+B", "", "A+B")>]
[<InlineData("A.B+C", "A", "B+C")>]
[<InlineData("A.B.C+D+E", "A.B", "C+D+E")>]
let ``parseType given inner type returns inner type list`` (str, ns: string, typeParts: string) =
    let actual = parseType str

    let expected: Result<FullyQualifiedTypeDescription, string> =
        Result.Ok
            { Namespace = ns.Split('.') |> List.ofArray |> List.filter (fun x -> x <> "")
              TypeDescription =
                typeParts.Split('+')
                |> List.ofArray
                |> List.map (fun str -> { TypeName = str; TypeVariables = [] }) }

    actual |> should equal expected

[<Theory>]
[<InlineData(":")>]
[<InlineData("]")>]
[<InlineData(">")>]
[<InlineData(",")>]
[<InlineData("'")>]
let ``parseType give type followed by ending char returns ok`` (char) =
    let actual = parseType $"str{char}"

    Result.isOk actual |> should equal true

[<Fact>]
let ``parseType given inner type with generics returns inner type list`` () =
    let actual = parseType "A.B<C>+D<E.F+G>"

    let expected: Result<FullyQualifiedTypeDescription, string> =
        Result.Ok
            { Namespace = [ "A" ]
              TypeDescription =
                [ { TypeName = "B"
                    TypeVariables =
                      [ { Namespace = []
                          TypeDescription = [ { TypeName = "C"; TypeVariables = [] } ] } ] }
                  { TypeName = "D"
                    TypeVariables =
                      [ { Namespace = [ "E" ]
                          TypeDescription =
                            [ { TypeName = "F"; TypeVariables = [] }
                              { TypeName = "G"; TypeVariables = [] } ] } ] } ] }

    actual |> should equal expected
