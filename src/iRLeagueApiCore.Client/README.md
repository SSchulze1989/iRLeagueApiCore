# iRLeagueApiCore.Client

Simple client for using the iRLeagueManager API from a .NET application

## Usage

### Register service

```csharp
services.AddLeagueApiClient();
```

### Use client

```csharp
client = service.GetRequiredService<ILeagueApiClient>();

var leagues = await client.Leagues().Get();
```

### Log in as API user

```csharp
await client.LogIn("Username", "Password");
```
