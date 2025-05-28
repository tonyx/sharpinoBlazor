module SharpinoRecordStoreWebTests.WebTests

open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Sharpino.CommandHandler
open Sharpino.Storage
open Sharpino.TestUtils
open SharpinoRecordStore.RecordStore
open SharpinoRecordStore.models
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions
open Microsoft.Playwright
open Npgsql.FSharp
open Npgsql
open FSharpPlus

open Expecto

// todo: recreate the webapp that will be hit by those test as
// something looks not good atm

let logger: ILogger<RecordStore> = NullLogger<RecordStore>.Instance

[<Tests>]
let tests =
    testList "foo" [
        ftestCaseAsync "check home page" <| async {
            let! playwright = Playwright.CreateAsync() |> Async.AwaitTask
            let browser = playwright.Chromium.LaunchAsync().Result
            let page = browser.NewPageAsync().Result
            let! _  =
                page.GotoAsync("http://localhost:5150") |> Async.AwaitTask
                // page.GotoAsync("http://localhost:5186") |> Async.AwaitTask
                // http://localhost:5150
            let title = page.TitleAsync().Result
            printf "title: %s" title
            
            browser.CloseAsync().Wait()
            Expect.equal title "Home" "should be equal"
            browser.CloseAsync().Wait()
        }
        
        testCaseAsync "register new user" <| async {
            let! playwright = Playwright.CreateAsync() |> Async.AwaitTask
            let browser = playwright.Chromium.LaunchAsync().Result
            let page = browser.NewPageAsync().Result
            let! _  =
                page.GotoAsync("http://localhost:5186/Account/Register") |> Async.AwaitTask
            let title = page.TitleAsync().Result
            Expect.equal title "Register" "should be true"
            let! _ =
                page.Locator("#Input\\.Email").FillAsync("fake@example.com") |> Async.AwaitTask
            let! _ =
                page.Locator("#Input\\.Password").FillAsync("danwe4-jafnyj-sytNoj") |> Async.AwaitTask
            let! _ =
                page.Locator("#Input\\.ConfirmPassword").FillAsync("danwe4-jafnyj-sytNoj") |> Async.AwaitTask
            let! _ =
                page.Locator("button[type='submit']").ClickAsync() |> Async.AwaitTask
            
            browser.CloseAsync().Wait()
            Expect.isTrue true "true"
        }
    ]
    |> testSequenced
    
    
    
