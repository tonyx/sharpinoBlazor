namespace sharpinoRecordStore.models
open Sharpino.Core
open Sharpino.Commons

module UserEvents =

    type UserEvents =
        | Deactivated
        | Activated
        | NameUpdated of string
        | EmailUpdated of string

            interface Event<User> with
                member this.Process user =
                    match this with
                    | Deactivated -> user.Deactivate()
                    | Activated -> user.Activate()
                    | NameUpdated newName -> user.UpdateName newName
                    | EmailUpdated newEmail -> user.UpdateEmail newEmail
        member this.Serialize =
            jsonPSerializer.Serialize this
        static member Deserialize (json: string) =
            jsonPSerializer.Deserialize<UserEvents> json

