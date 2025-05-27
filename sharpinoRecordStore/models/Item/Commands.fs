
namespace sharpinoRecordStore.models
open FsToolkit.ErrorHandling
open Sharpino.Core
open sharpinoRecordStore.models.ItemEvents
open System

type ItemCommand =
    | GiveTo of Guid
    | DeleteBy of Guid
    
    interface AggregateCommand<Item, ItemEvents> with
        member this.Execute item =
            match this with
            | GiveTo other ->
                item.GivesTo other
                |> Result.map (fun i -> (i, [GivenTo other])) 
            | DeleteBy owner ->
                item.DeleteBy owner
                |> Result.map (fun i -> (i, [DeletedBy owner]))
        member this.Undoer =
            None