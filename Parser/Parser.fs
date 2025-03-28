module Parser

open FParsec

type TypeDescription = {
    Namespace : string list
    TypeName : string
    TypeVariables : TypeDescription list
}

let allowedTypeNameChar = letter <|> digit <|> pchar '_'

let namespaceOrTypeName =
    letter .>>. many allowedTypeNameChar
    |>> fun (head, rest) -> System.String.Concat(head :: rest)

let numberedTypeVars  =
    attempt (pchar '`' >>. pint32)
    |>> fun count -> [1..count]
    |>> List.map (fun n -> {
        Namespace = []
        TypeName = $"T{n}"
        TypeVariables = []
    })

let (parseFullType : Parser<TypeDescription,unit>), parseFullTypeRef = createParserForwardedToRef()

let explicitNumberedTypeVars =
    between
        (pchar '`' >>. many1 digit .>> pchar '[')
        (pchar ']')
        (sepBy1 parseFullType (spaces .>> pchar ',' .>> spaces))
    |> attempt

let explicitTypeVars =
    between
        (pchar '<')
        (pchar '>')
        (sepBy1 parseFullType (spaces .>> pchar ',' .>> spaces))
    |> attempt

let typeVars =
    explicitNumberedTypeVars <|> numberedTypeVars <|> explicitTypeVars

parseFullTypeRef.Value <-
    parse {
        let! ns = (many (attempt (namespaceOrTypeName .>> pchar '.')))
        let! tn = namespaceOrTypeName
        let! tv = (opt typeVars)
        do! followedBy (eof <|> spaces1 <|> (anyOf [']';'>';','; '\''] >>% ()))

        return {
            Namespace = ns
            TypeName = tn
            TypeVariables = tv |> Option.defaultValue []
        }
    }

let complicatedType: Parser<TypeDescription, unit> =
    parseFullType >>= (
        fun (t : TypeDescription) ->
            if t.Namespace = [] && t.TypeVariables = []
            then fail "Type does not have namespace or type variables"
            else preturn t
    )

type MessagePart = Text of string | Type of TypeDescription

let messageText: Parser<MessagePart, unit> =
    many1Till anyChar (eof <|> (lookAhead complicatedType >>% ()))
    |>> (System.String.Concat >> Text)

let parseMessage (str: string) : Result<MessagePart list, string> =
    match run (many ((complicatedType |>> Type |> attempt) <|> messageText)) str with
    | Success(result, _, _) -> Result.Ok result
    | Failure(err, _, _) -> Result.Error err
