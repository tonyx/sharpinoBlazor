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

        new (configuration: IConfiguration, logger: ILogger<RecordStore>) as this =
            let pgEventStore = PgStorage.PgEventStore (configuration.GetValue<string>("sharpinoDb:Connection"))
            let usersViewer = getAggregateStorageFreshStateViewer<User, UserEvents,string> pgEventStore
            let itemsViewer = getAggregateStorageFreshStateViewer<Item, ItemEvents,string> pgEventStore 
            RecordStore (configuration, logger, pgEventStore, doNothingBroker, usersViewer, itemsViewer)

        member this.AddUser (user: User) =
            result
                {
                    return!
                        user
                        |> runInit<User, UserEvents, string> eventStore eventBroker
                }
        
        member this.AddUserAsync (user: User) =
            taskResult
                {
                    return!
                        user
                        |> runInit<User, UserEvents, string> eventStore eventBroker
                }
        
        member this.AddItemAsync (item: Item) =
            taskResult
                {
                    return!
                        item
                        |> runInit<Item, ItemEvents, string> eventStore eventBroker
                }
        
        member this.UserGivesItemToAnotherUser (giverId: Guid, itemId: Guid, receiverId: Guid) =
            result
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
                
        member this.UserGivesItemToAnotherUserAsync (giverId: Guid, itemId: Guid, receiverId: Guid) =
            task
                {
                    return this.UserGivesItemToAnotherUser (giverId, itemId, receiverId)
                }
        
        member this.GetAllItemsOfUser (userId: Guid) =
            result
                {
                    let! items =
                        StateView.getAllAggregateStates<Item, ItemEvents, string> eventStore
                    let result = items |>> snd |> List.filter (fun i -> i.OwnerId = userId && not i.Deleted)
                    return result
                }
       
        member this.DeleteItemByUser (itemId: Guid, userId: Guid) =
            result
                {
                    let! item = this.GetItem itemId
                    let! user = this.GetUser userId
                    return
                        ItemCommand.DeleteBy userId
                        |> runAggregateCommand<Item, ItemEvents, string> item.Id eventStore eventBroker 
                }
        member this.DeleteItemByUserAsync (itemId: Guid, userId: Guid) =
            task
                {
                    return this.DeleteItemByUser (itemId, userId)
                }        
                
        member this.GetAllItemsOfUserAsync (userId: Guid) =
            task
                {
                    return this.GetAllItemsOfUser userId 
                }
       
        member this.GetUser (id: Guid): Result<User, string> =
            usersViewer id |> Result.map snd
            
        member this.GetItem (id: Guid): Result<Item, string> =
            itemsViewer id |> Result.map snd
        
