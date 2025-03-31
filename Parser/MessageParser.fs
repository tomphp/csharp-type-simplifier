module Parser.MessageParser

open FParsec
open TypeParser

type MessagePart =
    | Text of string
    | Type of TypeDescription

let private messageText: Parser<MessagePart, unit> =
    many1Till anyChar (eof <|> (lookAhead parseType >>% ()))
    |>> (System.String.Concat >> Text)

let parseMessage (str: string) : Result<MessagePart list, string> =
    match run (many ((parseType |>> Type |> attempt) <|> messageText)) str with
    | Success(result, _, _) -> Result.Ok result
    | Failure(err, _, _) -> Result.Error err
