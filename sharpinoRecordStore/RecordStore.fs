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
            usersViewer: AggregateViewer<User>
        ) =

        new (configuration: IConfiguration, logger: ILogger<RecordStore>) as this =
            let pgEventStore = PgStorage.PgEventStore (configuration.GetValue<string>("sharpinoDb:Connection"))
            let usersViewer = getAggregateStorageFreshStateViewer<User, UserEvents,string> pgEventStore 
            RecordStore (configuration, logger, pgEventStore, doNothingBroker, usersViewer)

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
        
        member this.GetUser (id: Guid) =
            usersViewer id |> Result.map snd
