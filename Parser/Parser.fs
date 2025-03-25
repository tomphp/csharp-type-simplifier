module Parser

open FParsec

type TypeDescription = {
    Namespace : string list
    TypeName : string
    TypeVariables : TypeDescription list
}

let allowedChar = letter <|> digit <|> pchar '_'

let namespaceOrTypeName =
    letter .>>. many allowedChar
    |>> fun (head, rest) -> System.String.Concat(head :: rest)

let numberedTypeVars  =
    pchar '`' >>. pint32
    |>> fun count -> [1..count]
    |>> List.map (fun n -> {
        Namespace = []
        TypeName = $"T{n}"
        TypeVariables = []
    })

let (parseFullType : Parser<TypeDescription,unit>), parseFullTypeRef = createParserForwardedToRef()

let explicitTypeVars =
    between (pchar '<') (pchar '>') (sepBy1 parseFullType (pchar ','))

let typeVars =
    numberedTypeVars <|> explicitTypeVars

parseFullTypeRef.Value <-
    parse {
        let! ns = (many (attempt (namespaceOrTypeName .>> pchar '.')))
        let! tn = namespaceOrTypeName
        let! tv = (opt typeVars)
        do! followedBy (eof <|> (anyOf ['>';','] >>% ()))

        return {
            Namespace = ns
            TypeName = tn
            TypeVariables = tv |> Option.defaultValue []
        }
    }

let parseType (str : string) : Result<TypeDescription, string> =
    match run parseFullType str with
    | Success(result, _, _) -> Result.Ok result
    | Failure(err, _, _) -> Result.Error err
