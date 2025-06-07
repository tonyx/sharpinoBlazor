
namespace SharpinoRecordStore.models
open FsToolkit.ErrorHandling
open Sharpino.Core
open SharpinoRecordStore.models.ItemEvents
open System

type ItemCommand =
    | GiveTo of Guid
    | GiveTo2 of Guid * Guid
    | DeleteBy of Guid
    
    interface AggregateCommand<Item, ItemEvents> with
        member this.Execute item =
            match this with
            | GiveTo other ->
                item.GivesTo other
                |> Result.map (fun i -> (i, [GivenTo other])) 
            | GiveTo2 (owner, other) ->
                item.GivesTo2 (owner, other) 
                |> Result.map (fun i -> (i, [GivenTo2 (owner, other)])) 
            | DeleteBy owner ->
                item.DeleteBy owner
                |> Result.map (fun i -> (i, [DeletedBy owner]))
        member this.Undoer =
            None