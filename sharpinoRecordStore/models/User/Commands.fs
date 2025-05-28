namespace SharpinoRecordStore.models
open FsToolkit.ErrorHandling
open Sharpino.Core
open SharpinoRecordStore.models.UserEvents

module UserCommands =

    type UserCommand =
        | Deactivate
        | Activate
        | UpdateName of string
        | UpdateEmail of string

            interface AggregateCommand<User, UserEvents> with
                member this.Execute user =
                    match this with
                    | Deactivate -> 
                        user.Deactivate()
                        |> Result.map (fun u -> (u, [Deactivated])) // Assuming Deactivated is an event
                    | Activate -> 
                        user.Activate()
                        |> Result.map (fun u -> (u, [Activated])) // Assuming Activated is an event
                    | UpdateName newName -> 
                        user.UpdateName newName
                        |> Result.map (fun u -> (u, [NameUpdated newName])) // Assuming NameUpdated is an event
                    | UpdateEmail newEmail -> 
                        user.UpdateEmail newEmail
                        |> Result.map (fun u -> (u, [EmailUpdated newEmail])) // Assuming EmailUpdated is an event
                member this.Undoer =
                    None

