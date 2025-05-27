namespace sharpinoRecordStore.models
open sharpinoRecordStore.Definitions
open Sharpino.Commons
open Sharpino.Core
open System

module ItemEvents =
    type ItemEvents =
        | GivenTo of Guid
        | DeletedBy of Guid

        interface Event<Item> with
            member this.Process item =
                match this with
                | GivenTo other -> 
                    item.GivesTo other
                | DeletedBy owner ->
                    item.DeleteBy owner

        member this.Serialize =    
            jsonPSerializer.Serialize this
        static member Deserialize (json: string) =
            jsonPSerializer.Deserialize<ItemEvents> json