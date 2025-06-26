namespace SharpinoRecordStore

open System
open Sharpino.Core
open Sharpino
open Sharpino.Storage
open Sharpino.CommandHandler

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

open FsToolkit.ErrorHandling
open FSharpPlus
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result

open SharpinoRecordStore.models
open SharpinoRecordStore.models.UserEvents
open SharpinoRecordStore.models
open SharpinoRecordStore.models.ItemEvents

module RecordStore =

    let doNothingBroker: IEventBroker<_> =
        { 
            notify = None
            notifyAggregate = None
        }

    type RecordStore
        (
            logger: ILogger<RecordStore>,
            eventStore: IEventStore<string>,
            eventBroker: IEventBroker<string>,
            usersViewer: AggregateViewer<User>,
            itemsViewer: AggregateViewer<Item>
        ) =

        new (configuration: IConfiguration, logger: ILogger<RecordStore>) as this =
            let pgEventStore = PgStorage.PgEventStore (configuration.GetValue<string>("sharpinoDb:Connection"))
            let usersViewer = getAggregateStorageFreshStateViewer<User, UserEvents,string> pgEventStore
            let itemsViewer = getAggregateStorageFreshStateViewer<Item, ItemEvents,string> pgEventStore 
            RecordStore (logger, pgEventStore, doNothingBroker, usersViewer, itemsViewer)
                then
                    logger.LogInformation("RecordStore created")
                    // will snapshots all users to force upcast in case of new version
                    match
                        this.SnapshotAllItems() with
                        | Ok _ -> logger.LogInformation("Items snapshoted")
                        | Error ex -> logger.LogError(ex.ToString(), "Error snapshoting items")
        
        member this.AddUserAsync (user: User) =
            logger.LogInformation("Adding user")
            taskResult
                {
                    return!
                        user
                        |> runInit<User, UserEvents, string> eventStore eventBroker
                }
        
        member this.AddItemAsync (item: Item) =
            logger.LogInformation("Adding item")
            taskResult
                {
                    let! _  = this.GetUser item.OwnerId 
                    return!
                        item
                        |> runInit<Item, ItemEvents, string> eventStore eventBroker
                }
        member this.UserGivesItemToAnotherUserAsync (giverId: Guid, itemId: Guid, receiverId: Guid) =
            logger.LogInformation("User gives item to another user")
            taskResult
                {
                    let! giver = this.GetUser giverId
                    let! receiver = this.GetUser receiverId
                    let! item = this.GetItem itemId
                    return!    
                        ItemCommand.GiveTo2 (giverId, receiverId)
                        |> runAggregateCommand<Item, ItemEvents, string> item.Id eventStore eventBroker
                }
        
        member this.GetAllItemsOfUserAsync (userId: Guid) =
            logger.LogInformation("Getting all items of user")
            taskResult
                {
                    let! items =
                        StateView.getAllAggregateStates<Item, ItemEvents, string> eventStore
                    // let result = items |>> snd |> List.filter (fun i -> i.OwnerId = userId && not i.Deleted)
                    let result = items |>> snd |> List.filter (fun i -> i.OwnerId = userId)
                    return result
                }
       
        member this.DeleteItemByUserAsync (itemId: Guid, userId: Guid) =
            logger.LogInformation("Deleting item by user")
            taskResult
                {
                    let! item = this.GetItem itemId
                    let! user = this.GetUser userId
                    return!
                        ItemCommand.DeleteBy userId
                        |> runAggregateCommand<Item, ItemEvents, string> item.Id eventStore eventBroker 
                }
        
        member this.SnapshotAllItems() =
            logger.LogInformation("Snapshotting all items")
            result
                {
                    let! snapshotsAll =
                        StateView.getAllAggregateStates<Item, ItemEvents, string> eventStore
                        >>= List.traverseResultM (fun (_, item) -> mkAggregateSnapshot<Item, ItemEvents, string> eventStore item.Id)
                    return ()    
                }
                
        member this.GetUser (id: Guid): Result<User, string> =
            usersViewer id |> Result.map snd
            
        member this.GetItem (id: Guid): Result<Item, string> =
            itemsViewer id |> Result.map snd
        
