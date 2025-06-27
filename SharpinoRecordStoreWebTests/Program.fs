// For more information see https://aka.ms/fsharp-console-apps
module SharpinoRecordStore.WebTests
open System
open DotNetEnv
open Expecto
open Npgsql.FSharp
open Npgsql
open FSharpPlus

Env.Load() |> ignore
let password = Environment.GetEnvironmentVariable("password")
let cleanDb () =
    let connection =
        "Server=127.0.0.1;"+
        "Database=sharpino_sample_auth_test;" +
        "User Id=postgres;"+
        $"Password={password};"
    connection
        |> Sql.connect
        |> Sql.query (sprintf "DELETE from public.\"AspNetUsers\"")
        |> Sql.executeNonQuery

[<EntryPoint>]
let main argv =
    cleanDb () |> ignore
    Tests.runTestsInAssemblyWithCLIArgs ([]) argv
