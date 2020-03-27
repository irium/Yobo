namespace Yobo.Server

open Microsoft.Azure.WebJobs.Description
open System
open System.Security.Claims
open System.Threading.Tasks
open Yobo.Server.Auth
open Yobo.Server.UserAccount
open Yobo.Shared.Domain

type AuthQueries = {
    TryGetUserByEmail : string -> Task<Auth.Domain.Queries.AuthUserView option>
    TryGetUserById : Guid -> Task<Auth.Domain.Queries.BasicUserView option>
}

type AuthCommandHandler = {
    Register : Domain.CmdArgs.Register -> Task<unit>
    ActivateAccount : Domain.CmdArgs.Activate -> Task<unit>
    ForgottenPassword : Domain.CmdArgs.InitiatePasswordReset -> Task<unit>
    ResetPassword : Domain.CmdArgs.CompleteResetPassword -> Task<unit>
}

type AuthRoot = {
    CreateToken : Claim list -> string
    ValidateToken : string -> Claim list option
    VerifyPassword : string -> string -> bool
    CreatePasswordHash : string -> string
    Queries : AuthQueries
    CommandHandler : AuthCommandHandler
}

type UserAccountQueries = {
    TryGetUserInfo : Guid -> Task<Yobo.Shared.UserAccount.Domain.Queries.UserAccount option>
}

type UserAccountRoot = {
    Queries : UserAccountQueries
}

type UsersQueries = {
    GetAllUsers : unit -> Task<Yobo.Shared.Users.Domain.Queries.User list>
}

type UsersRoot = {
    Queries : UsersQueries
}

type CompositionRoot = {
    Auth : AuthRoot
    UserAccount : UserAccountRoot
    Users : UsersRoot
}    

module Attributes =
    [<Binding>]
    [<AttributeUsage(AttributeTargets.Parameter)>]
    type CompositionRootAttribute() = inherit Attribute()