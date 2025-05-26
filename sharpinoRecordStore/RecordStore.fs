namespace sharpinoRecordStore

open System
open Sharpino.Core
open Sharpino
open Microsoft.Extensions.Configuration
open Sharpino.CommandHandler
open FsToolkit.ErrorHandling

module RecordStore =
    open Sharpino.Storage
    open sharpinoRecordStore.models
    open Microsoft.Extensions.Logging
    open sharpinoRecordStore.models.UserEvents

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
        
        member this.GetUser (id: Guid) =
            usersViewer id |> Result.map snd
