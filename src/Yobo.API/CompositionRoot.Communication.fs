module Yobo.API.CompositionRoot.Communication

let private toAsync f = async { return f }

module Users =
    open Yobo.API.Users.Functions
    open Yobo.Libraries.Security 
    open Yobo.API.CompositionRoot
    open Yobo.Shared.Auth
    
    let api : Yobo.Shared.Users.Communication.API = {
        Login = login Services.Users.authenticator.Login (Services.Users.authorizator.CreateToken >> fun x -> x.AccessToken) >> toAsync
        GetUser = getUser Services.Users.authorizator.ValidateToken >> toAsync
        ResendActivation = resendActivation Services.CommandHandler.handle >> toAsync
        Register = register Services.CommandHandler.handle Password.createHash >> toAsync
        ActivateAccount = activateAccount Services.CommandHandler.handle Services.Users.queries.GetByActivationKey >> toAsync
    }