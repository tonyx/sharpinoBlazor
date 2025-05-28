# sharpinoBlazor

The classic blazor app based on a template that includes user registration is modified so that
a shadow copy of the Asp.net identity users is stored in the sharpino_recordstore database.

Key points: 
1. C# Blazor user interface with Asp.net identity and event sourced users
2. F# Sharpino based event sourcing
3. any .razor page will access the unique RecordStore instance via dependency injection

Note: 
you need this appsettings.json in the main directory
```json
{
"ConnectionStrings": {
"DefaultConnection": "Host=localhost;Database=sharpino_sample_auth;Username=postgresusername;Password=postgrespassword"
},
"SharpinoDb": {
"Connection": "Server=127.0.0.1;Database=sharpino_recordstore;User Id=safe;Password=safe"
},

"Logging": {
"LogLevel": {
"Default": "Information",
"Microsoft.AspNetCore": "Warning"
}
},
"AllowedHosts": "*"
}
```

you need also the following .env file in the sharpinoRecordStore subdirectory:
```
DATABASE_URL="postgres://postgresuser:postgrespassword@127.0.0.1:5432/sharpino_recordstore?sslmode=disable"
DATABASE_TEST="postgres://postgresuser:postgrespassword@127.0.0.1:5432/sharpino_recordstore_test?sslmode=disable"

```

finally: providing thet you have your postgresql database up and running, you can populate it with the following command
```
dbmate up 
```
to setup the test database:
```bash
dbmate -e DATABASE_TEST up

```

in the SharpinoRecordStoreTest directory you need a .env file with the password of the 'safe' user (or any other user with access to the database, specified in the Commons.fs file)
```
password=userpassword
```
you may avoid the database setup by using only the in-memory event store
in that case you can comment out the pgEventStore instance based entry in the instances list:

```fsharp
    let instances =
        [
        //     (fun () -> setUp pgEventStore), RecordStore (logger, pgEventStore, doNothingBroker, pgUsersViewer, pgItemViewer)
            (fun () -> setUp memEventStore), RecordStore (logger, memEventStore, doNothingBroker, memUsersViewer, memitemViewer)
        ]

```


from the sharpinoRecordStore subdirectory

## Various info:

The RecordStore.fs app is resolved as a singleton by using the dependency injection of .net by adding the following line in Program.cs
```csharp
builder.Services.AddSingleton<RecordStore.RecordStore>();
```

the ef (entity framework) migration files are adapted to the database that is used (postgres)

The legacy Asp.net identity users are stored in the sharpino_sample_auth database
The event sourced users are stored in the sharpino_recordstore database

So when a user is added then the equivalent user is created also as a sharpino event sourced object.
The application will just borrow the users (and, most important, their Id) from the Asp.net identity system. 
Any other event sourced entity of the domain may follow the structure of the models directory and an entry in
the business logic layer (RecordStore.fs).
Some computations expressions use the let! expressionn to bind to objects that they don't use: 
it is a way to verify that the objects exist.

## In progress:
Additional tests based on hitting the web app via the playwright library
a "test" profile should be used to run the webapp to be tested by the playwright tests (dotnet run -lp=test)
in that way serparate databases can be used for testing.
i.e. copes of the databases sharpino_sample_auth and sharpino_recordstore like
sharpino_sample_auth_test and sharpino_recordstore_test.






