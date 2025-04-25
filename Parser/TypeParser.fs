module Parser.TypeParser

open FParsec

type TypeDescription =
    { TypeName: string
      TypeVariables: FullyQualifiedTypeDescription list }

and FullyQualifiedTypeDescription =
    { Namespace: string list
      TypeDescription: TypeDescription list }

let private allowedTypeNameChar = letter <|> digit <|> pchar '_'

let private namespaceOrTypeName =
    letter .>>. many allowedTypeNameChar
    |>> fun (head, rest) -> System.String.Concat(head :: rest)

let private numberedTypeVars =
    attempt (pchar '`' >>. pint32)
    |>> fun count -> [ 1..count ]
    |>> List.map (fun n ->
        { Namespace = []
          TypeDescription =
            [ { TypeName = $"T{n}"
                TypeVariables = [] } ] })

let (fullyQualifiedTypeDescription: Parser<FullyQualifiedTypeDescription, unit>), fullyQualifiedTypeDescriptionRef =
    createParserForwardedToRef ()

let private explicitNumberedTypeVars =
    between
        (pchar '`' >>. many1 digit .>> pchar '[')
        (pchar ']')
        (sepBy1 fullyQualifiedTypeDescription (spaces .>> pchar ',' .>> spaces))
    |> attempt

let private explicitTypeVars =
    between (pchar '<') (pchar '>') (sepBy1 fullyQualifiedTypeDescription (spaces .>> pchar ',' .>> spaces))
    |> attempt

let private typeVars =
    explicitNumberedTypeVars <|> numberedTypeVars <|> explicitTypeVars

let private typeDescription =
    parse {
        let! name = namespaceOrTypeName
        let! vars = (opt typeVars) |>> Option.defaultValue []
        do! followedBy (eof <|> spaces1 <|> (anyOf [ ']'; '>'; ','; '\''; '+' ] >>% ()))

        return
            { TypeName = name
              TypeVariables = vars }
    }

fullyQualifiedTypeDescriptionRef.Value <-
    parse {
        let! ns = (many (attempt (namespaceOrTypeName .>> pchar '.')))
        let! typeDescription = sepBy1 typeDescription (pchar '+')

        return
            { Namespace = ns
              TypeDescription = typeDescription }
    }

let parseType: Parser<FullyQualifiedTypeDescription, unit> =
    fullyQualifiedTypeDescription
    >>= (fun (t: FullyQualifiedTypeDescription) ->
        if List.isEmpty t.Namespace && List.isEmpty t.TypeDescription[0].TypeVariables then // Todo
            fail "Type does not have namespace or type variables"
        else
            preturn t)
