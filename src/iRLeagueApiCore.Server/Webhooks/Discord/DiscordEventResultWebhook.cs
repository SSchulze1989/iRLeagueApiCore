using Aydsko.iRacingData.Leagues;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueApiCore.Server.Handlers.Results;
using System.Text;

namespace iRLeagueApiCore.Server.Webhooks.Discord;
public class DiscordEventResultWebhook : IEventResultWebhook
{
    private readonly ILogger<DiscordEventResultWebhook> logger;
    private readonly LeagueDbContext dbContext;
    private readonly IMediator mediator;

    public DiscordEventResultWebhook(ILogger<DiscordEventResultWebhook> logger, LeagueDbContext dbContext, IMediator mediator)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.mediator = mediator;
    }

    public async Task SendAsync(object? data, string url, CancellationToken cancellationToken = default)
    {
        if (data is not long resultEventId)
        {
            logger.LogError("Invalid data for EventResultWebhook: {Data}", data);
            return;
        }

        if (dbContext.LeagueProvider.LeagueId <= 0)
        {
            logger.LogError("LeagueId is not set in LeagueProvider");
            return;
        }

        // get event result data -> use handlers to get easy data access
        var results = await mediator.Send(new GetResultsFromEventRequest(resultEventId), cancellationToken);

        // Create discord embeds
        var embeds = new List<object>();
        var driverChampionship = results.FirstOrDefault();
        var teamChampionship = results.Skip(1).FirstOrDefault();
        var qualyResult = driverChampionship?.SessionResults.Skip(1).FirstOrDefault();
        var raceResult = driverChampionship?.SessionResults.Last();
        var leagueName = dbContext.LeagueProvider.HasLeagueName ? dbContext.LeagueProvider.LeagueName : string.Empty;
        if (string.IsNullOrEmpty(leagueName))
        {
            leagueName = (await dbContext.Leagues
                .Where(x => x.Id == dbContext.LeagueProvider.LeagueId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken)) ?? string.Empty;
        }
        if (driverChampionship is null)
        {
            logger.LogError("No driver championship result found for event {EventId}", resultEventId);
            return;
        }
        var fields = new List<Dictionary<string, object>>();
        if (raceResult is not null)
        {
            fields.Add(new()
            {
                ["name"] = "Podium",
                ["value"] = CreateaPodiumSummary(raceResult),
            });
            fields.Add(new()
            {
                ["name"] = "Race Results",
                ["value"] = CreateRaceResultsTable(raceResult),
            });
            fields.Add(new()
            {
                ["name"] = "",
                ["value"] = CreateRaceResultsTable2(raceResult, leagueName, resultEventId),
            });
        }

        fields.Add(new()
        {
            ["name"] = "Information",
            ["value"] = $"""
        📊 **Strength of Field**: {driverChampionship.StrengthOfField}
        📅 **Datum**: {DateTimeStamp(driverChampionship.Date)}
        """
        });


        var embed = new Dictionary<string, object>
        {
            ["title"] = driverChampionship.EventName,
            ["description"] = $"**{driverChampionship.TrackName}{(driverChampionship.ConfigName != "" ? " (" + driverChampionship.ConfigName + ")" : "")}**",
            ["color"] = 3066993,
            ["timestamp"] = DateTime.UtcNow,
            ["fields"] = fields,
            ["footer"] = new Dictionary<string, object>()
            {
                ["text"] = "Data provided by irleaguemanager.net",
            },
        };
        embeds.Add(embed);

        // Create Discord message
        var message = new Dictionary<string, object>()
        {
            ["embeds"] = embeds,
        };

        // Send Message to Discord webhook
        var httpClient = new HttpClient();
        var content = JsonContent.Create(message);

        logger.LogDebug("Sending Discord webhook to {Url} with content: {Content}", url, await content.ReadAsStringAsync(cancellationToken));
        var response = await httpClient.PostAsync(url, content, cancellationToken);
        logger.LogDebug("Discord webhook response: {StatusCode} - {Response}", response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    static void AppendColumnValue(StringBuilder sb, string value, int columnWidth)
    {
        var paddedValue = value.PadRight(columnWidth).Substring(0, columnWidth);
        sb.Append(paddedValue);
        sb.Append(" | ");
    }

    static string CreateQualyResultsTable(ResultModel result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```");
        sb.AppendLine("#  | Fahrer                 | Zeit       ");
        sb.AppendLine("---|------------------------|------------");
        foreach (var row in result.ResultRows.Take(15))
        {
            AppendColumnValue(sb, row.FinalPosition.ToString(), 2);
            AppendColumnValue(sb, $"{row.Firstname} {row.Lastname}", 22);
            AppendColumnValue(sb, $"{DateTime.Today.Add(row.QualifyingTime):mm:ss.fff}", 10);
            sb.Length -= 3;
            sb.Append('\n');
        }
        sb.AppendLine("```");
        return sb.ToString();
    }

    static string IntervalToString(Interval interval)
    {
        if (interval.Laps == 0)
        {
            return $"{DateTime.Today.Add(interval.Time):mm:ss.fff}";
        }
        return $"{interval.Laps}L";
    }

    static string LapTimeToString(TimeSpan lapTime)
    {
        if (lapTime > TimeSpan.Zero)
        {
            return $"{DateTime.Today.Add(lapTime):mm:ss.fff}";
        }
        return $"--:--.---";
    }

    static string DateTimeStamp(DateTime dateTime) => $"<t:{((DateTimeOffset)dateTime).ToUnixTimeSeconds()}:D>";

    static string CreateRaceResultsTable(ResultModel result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```");
        sb.AppendLine("Pos | Driver               | Interval  | Fast Lap  | Pts");
        sb.AppendLine("----|----------------------|-----------|-----------|----");
        foreach (var row in result.ResultRows.Take(14))
        {
            AppendColumnValue(sb, $"{row.FinalPosition}", 3);
            AppendColumnValue(sb, $"{row.Firstname} {row.Lastname}", 20);
            AppendColumnValue(sb, IntervalToString(row.Interval), 9);
            AppendColumnValue(sb, LapTimeToString(row.FastestLapTime), 9);
            AppendColumnValue(sb, $"{row.TotalPoints}", 3);
            sb.Length -= 3;
            sb.Append('\n');
        }
        sb.AppendLine("```");
        return sb.ToString();
    }

    static string CreateRaceResultsTable2(ResultModel result, string leagueName, long eventId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```");
        foreach (var row in result.ResultRows.Skip(14).Take(14))
        {
            AppendColumnValue(sb, $"{row.FinalPosition}", 3);
            AppendColumnValue(sb, $"{row.Firstname} {row.Lastname}", 20);
            AppendColumnValue(sb, IntervalToString(row.Interval), 9);
            AppendColumnValue(sb, LapTimeToString(row.FastestLapTime), 9);
            AppendColumnValue(sb, $"{row.TotalPoints}", 3);
            sb.Length -= 3;
            sb.Append('\n');
        }
        sb.AppendLine("```");
        sb.AppendLine($"Full results at: https://irleaguemanager.net/{leagueName}/Results/Events/{eventId}");
        return sb.ToString();
    }

    static string CreateStandingsTable(StandingsModel standings, string leagueName, bool includeFooter = false)
    {
        var sb = new StringBuilder();
        var isTeamStanding = standings.IsTeamStanding;
        sb.AppendLine("```");
        AppendColumnValue(sb, "Pos", 3);
        if (isTeamStanding)
        {
            AppendColumnValue(sb, "Team", 24);
        }
        else
        {
            AppendColumnValue(sb, "Driver", 24);
        }
        AppendColumnValue(sb, "Points", 6);
        sb.Append('\n');
        sb.AppendLine("----|--------------------------|-------");
        foreach (var row in standings.StandingRows.Take(20))
        {
            AppendColumnValue(sb, $"{row.Position}", 3);
            if (isTeamStanding)
            {
                AppendColumnValue(sb, $"{row.TeamName}", 24);
            }
            else
            {
                AppendColumnValue(sb, $"{row.Firstname} {row.Lastname}", 24);
            }
            AppendColumnValue(sb, $"{row.TotalPoints}", 6);
            sb.Length -= 3;
            sb.Append('\n');
        }
        sb.AppendLine("```");
        if (includeFooter)
        {
            sb.AppendLine($"Full standings at: https://irleaguemanager.net/{leagueName}/Standings");
        }
        return sb.ToString();
    }

    static string CreateaPodiumSummary(ResultModel result)
    {
        var sb = new StringBuilder();
        var row = result.ResultRows.FirstOrDefault();
        if (row is not null)
        {
            sb.AppendLine($"🥇 {row.Firstname} {row.Lastname}");
        }
        row = result.ResultRows.Skip(1).FirstOrDefault();
        if (row is not null)
        {
            sb.AppendLine($"🥈 {row.Firstname} {row.Lastname}  +{IntervalToString(row.Interval)}");
        }
        row = result.ResultRows.Skip(2).FirstOrDefault();
        if (row is not null)
        {
            sb.AppendLine($"🥉 {row.Firstname} {row.Lastname}  +{IntervalToString(row.Interval)}");
        }
        row = result.ResultRows.OrderBy(x => x.QualifyingTime).Where(x => x.QualifyingTime > TimeSpan.Zero).FirstOrDefault();
        if (row is not null)
        {
            sb.AppendLine();
            sb.AppendLine("**Pole Position**");
            sb.AppendLine($"⏱️ {row.Firstname} {row.Lastname} - {LapTimeToString(row.QualifyingTime)}");
        }
        row = result.ResultRows.OrderBy(x => x.FastestLapTime).Where(x => x.FastestLapTime > TimeSpan.Zero).FirstOrDefault();
        if (row is not null)
        {
            sb.AppendLine();
            sb.AppendLine("**Fastest Lap**");
            sb.AppendLine($"⏱️ {row.Firstname} {row.Lastname} - {LapTimeToString(row.FastestLapTime)}");
        }

        return sb.ToString();
    }
}
