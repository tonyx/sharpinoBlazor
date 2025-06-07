
namespace rec SharpinoRecordStore.models
open Sharpino.Commons
open FsToolkit.ErrorHandling
open Sharpino.Core
open Sharpino
open System

type ItemType =
| Book
| Movie
| Game
| Music
    static member FromString (s: string) =
        match s.ToLower() with
        | "book" -> Book
        | "movie" -> Movie
        | "game" -> Game
        | "music" -> Music
        | _ -> Book
    static member GetAllTypes () =
        [Book; Movie; Game; Music]     
    member this.ToString =
        match this with
        | Book -> "Book"
        | Movie -> "Movie"
        | Game -> "Game"
        | Music -> "Music"
        
type Item = {
    Id: Guid
    OwnerId: Guid
    Name: string
    ItemType: ItemType
    Deleted: bool
}
    with    
        static member Create ownerId name itemType =
            { Id = Guid.NewGuid(); OwnerId = ownerId; Name = name;  ItemType = itemType; Deleted = false }
        
        [<Obsolete("This method is deprecated, use GivesTo2 instead.")>]
        member this.GivesTo other =
            { this with OwnerId = other } |> Ok
        
        member this.GivesTo2 (owner: Guid, other: Guid) =
            result
                {
                        do!
                            this.OwnerId = owner
                            |> Result.ofBool "not owner"
                        return
                            {
                                this with OwnerId = other
                            }
                }
        member this.DeleteBy ownerId = 
            result
                {
                    do!
                        this.OwnerId = ownerId
                        |> Result.ofBool "not owner"
                    return
                        {
                            this with Deleted = true
                        }
                }

        member this.Serialize =
            jsonPSerializer.Serialize this

        static member Deserialize (json: string) =
            let deserialized =
                jsonPSerializer.Deserialize<Item> json
            match deserialized with
            | Ok x -> Ok x
            | Error e -> 
                match (jsonPSerializer.Deserialize<Item001> json) with
                    | Ok x -> Ok (x.Upcast())
                    | Error e1 -> Error (e + e1)

        static member StorageName = "_item"
        static member Version = "_01"
        static member SnapshotsInterval = 100

        interface Aggregate<string> with
            member this.Id = this.Id
            member this.Serialize = this.Serialize

type Item001 = {
    Id: Guid
    OwnerId: Guid
    Name: string
    ItemType: ItemType
}
    with
        static member Deserialize (json: string) =
            jsonPSerializer.Deserialize<Item001> json
        member this.Upcast(): Item =
            {
                Id = this.Id
                OwnerId = this.OwnerId
                Name = this.Name
                ItemType = this.ItemType
                Deleted = false
            }
    
