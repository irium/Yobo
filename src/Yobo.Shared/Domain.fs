﻿module Yobo.Shared.Domain

open Yobo.Shared.Validation

type AuthenticationError =
    | InvalidLoginOrPassword
    | InvalidOrExpiredToken
    | EmailAlreadyRegistered
    | AccountAlreadyActivatedOrNotFound
    | InvalidPasswordResetKey
    
module AuthenticationError =
    let explain = function
        | InvalidLoginOrPassword -> "Nesprávně vyplněný email nebo heslo."
        | InvalidOrExpiredToken -> "Token není validní nebo již vypršela jeho platnost."
        | EmailAlreadyRegistered -> "Tento email je již v systému registrován."
        | AccountAlreadyActivatedOrNotFound -> "Tento účet je již zaktivován nebo byl zadán neplatný aktivační klíč."
        | InvalidPasswordResetKey -> "Kód pro nastavení nového hesla je nesprávný, nebo byl již použit."


type ServerError =
    | Exception of string
    | Validation of ValidationError list
    | Authentication of AuthenticationError

exception ServerException of ServerError

module ServerError =
    let explain = function
        | Exception e -> e
        | Validation errs ->
            errs
            |> List.map ValidationError.explain
            |> String.concat ", "
        | Authentication e -> e |> AuthenticationError.explain
        
    let failwith (er:ServerError) = raise (ServerException er)
    
    let ofOption err (v:Option<_>) =
        v
        |> Option.defaultWith (fun _ -> err |> failwith)
    
    let ofResult<'a> (v:Result<'a,ServerError>) =
        match v with
        | Ok v -> v
        | Error e -> e |> failwith

    let validate (validationFn:'a -> ValidationError list) (value:'a) =
        match value |> validationFn with
        | [] -> value
        | errs -> errs |> Validation |> failwith


type ServerResult<'a> = Result<'a, ServerError>

module ServerResult =
    let ofValidation (validationFn:'a -> ValidationError list) (value:'a) : ServerResult<'a> =
        match value |> validationFn with
        | [] -> Ok value
        | errs -> errs |> Validation |> Error

type ServerResponse<'a> = Async<ServerResult<'a>>
type SecuredParam<'a> = {
    Token : string
    Parameter : 'a
}