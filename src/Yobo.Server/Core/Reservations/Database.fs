module Yobo.Server.Core.Reservations.Database

open System
open System.Data
open FSharp.Control.Tasks.V2
open Dapper.FSharp
open Dapper.FSharp.MSSQL
open Yobo.Libraries.Tasks
open Yobo.Shared.Core.Domain
open Yobo.Shared.Errors

module Queries =
    open Yobo.Server.Auth.Database
    open Yobo.Shared.Core.Reservations.Domain
    open Yobo.Server.Core.Database
    open Yobo.Shared.Core.Domain.Queries
    
    
    
    let private tryGetUserById (conn:IDbConnection) (userId:Guid) =
        select {
            table Tables.Users.name
            where (eq "Id" userId)
        }
        |> conn.SelectAsync<Tables.Users>
        |> Task.map Seq.tryHead
                
    let getLessons (conn:IDbConnection) userId (dFrom:DateTimeOffset,dTo:DateTimeOffset) =
        task {
            let! userInfo = tryGetUserById conn userId
            let! res =
                select {
                    table Tables.Lessons.name
                    leftJoin Tables.LessonReservations.name "LessonId" "Lessons.Id"
                    where (ge "Lessons.StartDate" dFrom + le "Lessons.StartDate" dTo)
                }
                |> conn.SelectAsyncOption<Tables.Lessons, Tables.LessonReservations>
            return
                res
                |> List.ofSeq
                |> List.groupBy fst
                |> List.map (fun (lsn,gr) ->
                    
                    let res = gr |> List.choose snd
                    let userRes =
                        res
                        |> List.tryFind (fun x -> x.UserId = userId)
                        |> Option.map (fun x -> LessonPayment.fromUseCredits x.UseCredits)
                    
                    let credits = userInfo |> Option.map (fun x -> x.Credits) |> Option.defaultValue 0
                    let creditsExpiration = userInfo |> Option.bind (fun x -> x.CreditsExpiration)
                    let cashBlocked = userInfo |> Option.map (fun x -> x.CashReservationBlockedUntil) |> Option.defaultValue (Some DateTimeOffset.MaxValue)
                    let lsnStatus = LessonStatus.getLessonStatus lsn.StartDate lsn.IsCancelled lsn.Capacity res.Length
                    {
                        Id = lsn.Id
                        StartDate = lsn.StartDate
                        EndDate = lsn.EndDate
                        Name = lsn.Name
                        Description = lsn.Description
                        LessonStatus = lsnStatus
                        ReservationStatus = ReservationStatus.getReservationStatus lsnStatus lsn.StartDate lsn.CancellableBeforeStart userRes credits creditsExpiration cashBlocked
                    } : Queries.Lesson
                )
                |> List.sortBy (fun x -> x.StartDate)
        }
        
    let getWorkshops (conn:IDbConnection) (dFrom:DateTimeOffset,dTo:DateTimeOffset) =
        task {
            let! res =
                select {
                    table Tables.Workshops.name
                    where (ge "Workshops.StartDate" dFrom + le "Workshops.StartDate" dTo)
                }
                |> conn.SelectAsync<Tables.Workshops>
            return
                res
                |> List.ofSeq
                |> List.map (fun x ->
                    {
                        Id = x.Id
                        StartDate = x.StartDate
                        EndDate = x.EndDate
                        Name = x.Name
                        Description = x.Description
                    } : Queries.Workshop
                )
                |> List.sortBy (fun x -> x.StartDate)
        }