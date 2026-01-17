# 🎮 iRLeagueApi.Core

| Branch | Status |
| ------ | ------ |
| main | [![.NET](https://github.com/SSchulze1989/iRLeagueApiCore/actions/workflows/dotnet_main_push.yml/badge.svg?branch=main&event=push)](https://github.com/SSchulze1989/iRLeagueApiCore/actions/workflows/dotnet_main_push.yml) |
| develop | [![.NET](https://github.com/SSchulze1989/iRLeagueApiCore/actions/workflows/dotnet_develop.yml/badge.svg?branch=develop&event=push)](https://github.com/SSchulze1989/iRLeagueApiCore/actions/workflows/dotnet_develop.yml) |

Live development server: https://irleaguemanager.net/api/swagger

A full backend implementation for the iRLeagueManager service & API - used by the website and clients to manage leagues, schedules, results and more.

---

## Table of contents
- [Projects](#projects)
- [Usage examples](#usage-examples---api-client)
- [Development server / API docs](#development-server--api-docs)
- [Requirements](#requirements)
- [Contributing](#contributing)
- [License](#license)

---

## Projects
- 🗄️ [iRLeagueDatabaseCore](https://github.com/SSchulze1989/iRLeagueApiCore/tree/develop/src/iRLeagueDatabaseCore)  
  Data model driven database structure using EntityFramework & MySQL. Entity models configure table columns and foreign keys. Includes EF migrations for safe upgrades.
- ⚙️ [iRLeagueApiCore.Server](https://github.com/SSchulze1989/iRLeagueApiCore/tree/develop/src/iRLeagueApiCore.Server)  
  The backend server executable: API routing, configurations, background services and request handlers.
  - Controllers — ASP.NET Core REST API routing
  - Handlers — Business logic using MediatR
  - Validation — FluentValidation rules for requests & models
  - Filters — Controller attributes for behavior customization
- 📦 [iRLeagueApiCore.Common](https://github.com/SSchulze1989/iRLeagueApiCore/tree/develop/src/iRLeagueApiCore.Common)  
  Shared models and DTOs used by server and client.
- 🧩 [iRLeagueApiCore.Client](https://github.com/SSchulze1989/iRLeagueApiCore/tree/develop/src/iRLeagueApiCore.Client)  
  Simple .NET client to consume the iRLeagueManager API.

---

## Usage examples - API Client
Register the client service:
```csharp
services.AddLeagueApiClient();
```

Resolve and use the client:
```csharp
var client = serviceProvider.GetRequiredService<ILeagueApiClient>();
var leagues = await client.Leagues().Get();
```

Log in as an API user:
```csharp
await client.LogIn("Username", "Password");
```

---

## Development server / API docs
🚀 The API Swagger UI (development server) is available at:
https://irleaguemanager.net/api/swagger

Use the Swagger UI to explore endpoints, models and try requests.

---

## Requirements
- .NET SDK (check project files for the exact target framework)
- MySQL or compatible database provider
- Recommended: use EF Core migrations from iRLeagueDatabaseCore to initialize or update database schema

---

## Contributing
Thanks for considering contributions! Please:
- Fork the repo
- Create feature branches
- Follow existing code style and patterns (MediatR handlers, FluentValidation, etc.)
- Open PRs against `develop` for review

---

## License
This project is licensed under the terms provided in the repository (check LICENSE file).

---
