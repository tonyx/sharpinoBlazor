namespace sharpinoRecordStore

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

open sharpinoRecordStore.models
open sharpinoRecordStore.models.UserEvents
open sharpinoRecordStore.models
open sharpinoRecordStore.models.ItemEvents

module RecordStore =

    let doNothingBroker: IEventBroker<_> =
        { 
            notify = None
            notifyAggregate = None
        }

    type RecordStore
        (
            configuration: IConfiguration,
            logger: ILogger<RecordStore>,
            eventStore: IEventStore<string>,
            eventBroker: IEventBroker<string>,
            usersViewer: AggregateViewer<User>,
            itemsViewer: AggregateViewer<Item>
        ) =

        new (configuration: IConfiguration, logger: ILogger<RecordStore>) =
            let pgEventStore = PgStorage.PgEventStore (configuration.GetValue<string>("sharpinoDb:Connection"))
            let usersViewer = getAggregateStorageFreshStateViewer<User, UserEvents,string> pgEventStore
            let itemsViewer = getAggregateStorageFreshStateViewer<Item, ItemEvents,string> pgEventStore 
            RecordStore (configuration, logger, pgEventStore, doNothingBroker, usersViewer, itemsViewer)
        
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
                    do!
                        item.OwnerId = giver.Id
                        |> Result.ofBool "not owner"
                    return!    
                        (ItemCommand.GiveTo receiverId)
                        |> runAggregateCommand<Item, ItemEvents, string> item.Id eventStore eventBroker
                }
        
        member this.GetAllItemsOfUserAsync (userId: Guid) =
            logger.LogInformation("Getting all items of user")
            taskResult
                {
                    let! items =
                        StateView.getAllAggregateStates<Item, ItemEvents, string> eventStore
                    let result = items |>> snd |> List.filter (fun i -> i.OwnerId = userId && not i.Deleted)
                    return result
                }
       
        member this.DeleteItemByUserAsync (itemId: Guid, userId: Guid) =
            logger.LogInformation("Deleting item by user")
            taskResult
                {
                    let! item = this.GetItem itemId
                    let! user = this.GetUser userId
                    return
                        ItemCommand.DeleteBy userId
                        |> runAggregateCommand<Item, ItemEvents, string> item.Id eventStore eventBroker 
                }
                
        member this.GetUser (id: Guid): Result<User, string> =
            usersViewer id |> Result.map snd
            
        member this.GetItem (id: Guid): Result<Item, string> =
            itemsViewer id |> Result.map snd
        
