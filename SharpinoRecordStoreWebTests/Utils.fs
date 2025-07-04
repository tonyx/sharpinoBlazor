module SharpinoRecordStoreWebTests.Utils

open System
open Sharpino
open Sharpino.Cache
open Sharpino.Storage
open SharpinoRecordStore.models
open DotNetEnv
open Sharpino
open Sharpino.Storage
Env.Load() |> ignore
let password = Environment.GetEnvironmentVariable("password")

let connection =
    "Server=127.0.0.1;"+
    "Database=sharpino_recordstore_test;" +
    "User Id=postgres;"+
    $"Password={password}";

let pgEventStore:IEventStore<string> = PgStorage.PgEventStore connection

let setUp (eventStore: IEventStore<string>) =
    eventStore.Reset User.Version User.StorageName
    eventStore.ResetAggregateStream User.Version User.StorageName
    AggregateCache<User, string>.Instance.Clear() 

    eventStore.Reset Item.Version Item.StorageName
    eventStore.ResetAggregateStream Item.Version Item.StorageName
    AggregateCache<Item, string>.Instance.Clear()
