# sharpinoBlazor

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

#Various info:

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


