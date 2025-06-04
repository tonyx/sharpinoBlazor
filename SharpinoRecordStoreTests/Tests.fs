module Tests
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
open SharpinoRecordStoreTests.Commons
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions

open Expecto

let logger: ILogger<RecordStore> = NullLogger<RecordStore>.Instance

let doNothingBroker: IEventBroker<_> =
    { 
        notify = None
        notifyAggregate = None
    }

let pgUsersViewer = getAggregateStorageFreshStateViewer<User, UserEvents.UserEvents, string> pgEventStore
let pgItemViewer = getAggregateStorageFreshStateViewer<Item, ItemEvents.ItemEvents, string> pgEventStore
let memUsersViewer = getAggregateStorageFreshStateViewer<User, UserEvents.UserEvents, string> memEventStore
let memitemViewer = getAggregateStorageFreshStateViewer<Item, ItemEvents.ItemEvents, string> memEventStore

[<Tests>]
let tests =
    let instances =
        [
            (fun () -> setUp pgEventStore), RecordStore (logger, pgEventStore, doNothingBroker, pgUsersViewer, pgItemViewer)
            (fun () -> setUp memEventStore), RecordStore (logger, memEventStore, doNothingBroker, memUsersViewer, memitemViewer)
        ]
    testList "record store" [
        multipleTestCase "add and retrieve a user" instances <| fun (setUp,  recordStore) ->
            setUp()
            
            let userId = Guid.NewGuid()
            let user = User.Create userId "name" "email@email.com"
            let added =
                recordStore.AddUserAsync user
                |> Async.AwaitTask
                |> Async.RunSynchronously
                
            Expect.isOk added "should be ok"
            
            let userRetrieved =
                recordStore.GetUser userId |> Result.get
                
            Expect.equal userRetrieved user "should be equal"
            
        multipleTestCase "add user, add and retrieve the an item by that user" instances <| fun (setUp,  recordStore) ->
            setUp()
            let userId = Guid.NewGuid()
            let user = User.Create userId "name" "email@email.com"
            let userAdded =
                recordStore.AddUserAsync user
                |> Async.AwaitTask
                |> Async.RunSynchronously
          
            Expect.isOk userAdded "should be ok" 
             
            let item = Item.Create userId "name" ItemType.Book
            let itemAdded =
                recordStore.AddItemAsync item
                |> Async.AwaitTask
                |> Async.RunSynchronously
                
            Expect.isOk itemAdded "should be ok"
            
            let retrievedItems =
                recordStore.GetAllItemsOfUserAsync userId
                |> Async.AwaitTask
                |> Async.RunSynchronously
            Expect.isOk retrievedItems "should be ok"
            
            Expect.equal retrievedItems.OkValue [item] "should be equal"
                
    ]
    |> testSequenced
