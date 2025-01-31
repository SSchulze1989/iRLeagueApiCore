FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /source
COPY . .
RUN dotnet restore src/iRLeagueApiCore.Server -r linux-x64
RUN dotnet publish src/iRLeagueApiCore.Server -r linux-x64 -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-extra
WORKDIR /app
COPY --link --from=build /app .
USER $APP_UID
ENTRYPOINT ["./iRLeagueApiCore.Server"]