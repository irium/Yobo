module Yobo.Client.State

open Elmish
open Fable.Import
open Yobo.Client.Domain

let urlUpdate (result: Option<Router.Page>) state =
    match result with
    | None ->
        Browser.console.error("Error parsing url: " + Browser.window.location.href)
        state, Router.modifyUrl state.Page
    | Some page ->
        let state = { state with Page = page }
        let cmd =
            match page with
            | Router.Page.AccountActivation id -> id |>  AccountActivation.Domain.Msg.Activate |> Msg.AccountActivationMsg |> Cmd.ofMsg
            | _ -> Cmd.none
        state, cmd

let private mapUpdate fn1 fn2 (f,s) = (fn1 f), (s |> Cmd.map fn2)

let init result =
    urlUpdate result State.Init
 
let update (msg : Msg) (state : State) : State * Cmd<Msg> =
    match msg with
    | LoginMsg m -> Login.State.update m state.LoginState |> mapUpdate (fun s -> { state with LoginState = s }) Msg.LoginMsg
    | RegistrationMsg m -> Registration.State.update m state.RegistrationState |> mapUpdate (fun s -> { state with RegistrationState = s }) Msg.RegistrationMsg
    | AccountActivationMsg m -> AccountActivation.State.update m state.AccountActivationState |> mapUpdate (fun s -> { state with AccountActivationState = s }) Msg.AccountActivationMsg 