namespace SharpinoRecordStore.models
open sharpinoRecordStore.Definitions
open Sharpino.Commons
open Sharpino.Core
open System

module ItemEvents =
    type ItemEvents =
        | GivenTo of Guid
        | GivenTo2 of Guid * Guid
        | DeletedBy of Guid

        interface Event<Item> with
            member this.Process item =
                match this with
                | GivenTo other -> 
                    item.GivesTo other
                | GivenTo2 (owner, other) ->
                    item.GivesTo2 (owner, other)
                | DeletedBy owner ->
                    item.DeleteBy owner

        member this.Serialize =    
            jsonPSerializer.Serialize this
        static member Deserialize (json: string) =
            jsonPSerializer.Deserialize<ItemEvents> json