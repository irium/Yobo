﻿module Yobo.Client.Pages.Login.State

open Domain
open Elmish
open Fable.Core
open Feliz.Router
open Yobo.Client
open Yobo.Shared.Auth.Validation
open Yobo.Client.Server
open Yobo.Client.SharedView
open Yobo.Shared.Auth.Communication
open Yobo.Client.Forms

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match msg with
    | ErrorOccured e ->
        { model with IsLoading = false }, e |> ServerResponseViews.showErrorToast
    | FormChanged f ->
        { model with Form = model.Form |> ValidatedForm.updateWith f |> ValidatedForm.validateConditionalWith model.FormSent validateLogin }, Cmd.none
    | Login ->
        let model = { model with FormSent = true; Form = model.Form |> ValidatedForm.validateWith validateLogin }
        if model.Form |> ValidatedForm.isValid then
            { model with IsLoading = true }, Cmd.OfAsync.either authService.GetToken model.Form.FormData LoggedIn (exnToError >> Msg.ErrorOccured)
        else model, Cmd.none
    | LoggedIn token ->
        TokenStorage.setToken token
        { model with IsLoading = false; Form = Request.Login.init |> ValidatedForm.init },
            Cmd.batch [ ServerResponseViews.showSuccessToast "Byli jste úspěšně přihlášeni!"; Router.navigate Paths.Calendar ]