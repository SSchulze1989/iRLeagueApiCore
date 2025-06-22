FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

FROM base AS setup
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /source
COPY src src

FROM setup AS build
WORKDIR /source
RUN dotnet restore src/iRLeagueApiCore.Server -r linux-x64
RUN dotnet build src/iRLeagueApiCore.Server -c Release -r linux-x64 --no-restore
RUN dotnet publish src/iRLeagueApiCore.Server -r linux-x64 -o /app --no-build -c Release --self-contained false --no-restore

FROM setup AS build_migrations
WORKDIR /source
RUN dotnet ef migrations bundle --runtime linux-x64 --project src/iRLeagueDatabaseCore --configuration Release --self-contained
RUN mkdir /app
RUN cp efbundle /app/efbundle

FROM base AS final
COPY --link --from=build /app /app
COPY --link --from=build_migrations /app /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra
WORKDIR /app
COPY --link --from=final /app .
USER $APP_UID
ENTRYPOINT ["./iRLeagueApiCore.Server"]