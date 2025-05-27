namespace sharpinoRecordStore.models
open Sharpino.Commons
open Sharpino.Core
open System

type User = {
    Id: Guid
    Name: string
    Email: string
    Active: bool
} with
    static member Create id name email =
        { Id = id; Name = name; Email = email; Active = true }
    member this.Deactivate() =
        { this with Active = false } |> Ok
    member this.Activate() =
        { this with Active = true } |> Ok
    member this.UpdateName newName =
        if String.IsNullOrWhiteSpace(newName) then
            Error "Name cannot be empty."
        else
            { this with Name = newName } |> Ok
    member this.UpdateEmail newEmail =
        if String.IsNullOrWhiteSpace(newEmail) then
            Error "Email cannot be empty."
        else
            { this with Email = newEmail } |> Ok

   /// =====
    member this.Serialize =
        jsonPSerializer.Serialize this

    static member StorageName = "_user"
    static member Version = "_01"
    static member SnapshotsInterval = 100
    static member Deserialize (x: string) =
        jsonPSerializer.Deserialize<User> x

    interface Aggregate<string> with
        member this.Id = this.Id
        member this.Serialize = this.Serialize
