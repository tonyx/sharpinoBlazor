# sharpinoBlazor

The classic blazor app based on a template that includes user registration is modified so that
a shadow copy of the Asp.net identity users is stored in the sharpino_recordstore database
Not so pretty but it works.

Key points: 
1. C# Blazor user interface with Asp.net identity and event sourced users
2. F# Sharpino based event sourcing
3. Async call from any .razor page to the RecordStore.fs app via dependency injection (singleton)

when a user is added then the equivalent user is created also as a sharpino event sourced object.

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

```

finally: providing thet you have your postgresql database up and running, you can populate it with the following command
```
dbmate up 
```
from the sharpinoRecordStore subdirectory

## Various info:

The RecordStore.fs app is resolved as a singleton by using the dependency injection of .net by adding the following line in Program.cs
```csharp
builder.Services.AddSingleton<RecordStore.RecordStore>();
```

the ef migration files are adapted to the database that is used (postgres)

The legacy Asp.net identity users are stored in the sharpino_sample_auth database
The event sourced users are stored in the sharpino_recordstore database

So when a user is added then the equivalent user is created also as a sharpino event sourced object.
The application will just borrow the users (and, most important, their Id) from the Asp.net identity system. 
Any other event sourced entity of the domain will be likely modelled as the object, the events and the commands.


