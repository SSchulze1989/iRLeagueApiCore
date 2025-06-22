FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

FROM base AS build
WORKDIR /source
COPY src src
RUN dotnet restore src/iRLeagueApiCore.Server -r linux-x64
RUN dotnet build src/iRLeagueApiCore.Server -c Release -r linux-x64 --no-restore
RUN dotnet publish src/iRLeagueApiCore.Server -r linux-x64 -o /app --no-build -c Release --self-contained false --no-restore

FROM build AS build_migrations
RUN dotnet ef migrations bundle --runtime linux-x64  --project src/iRLeagueDatabaseCore --configuration Release --self-contained
COPY --link src/iRLeagueDatabaseCore/efbundle.exe /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra
WORKDIR /app
COPY --link --from=build_migrations /app .
USER $APP_UID
ENTRYPOINT ["./iRLeagueApiCore.Server"]