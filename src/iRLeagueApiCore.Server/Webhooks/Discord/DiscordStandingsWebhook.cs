using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.Server.Handlers.Standings;

namespace iRLeagueApiCore.Server.Webhooks.Discord;

public class DiscordStandingsWebhook : DiscordWebhook, IStandingsWebhook
{
    public DiscordStandingsWebhook(ILogger<DiscordWebhook> logger, LeagueDbContext dbContext, IMediator mediator, HttpClient httpClient) :
        base(logger, dbContext, mediator, httpClient)
    {
    }

    public override async Task SendAsync(object? data, string url, CancellationToken cancellationToken = default)
    {
        if (dbContext.LeagueProvider.LeagueId <= 0)
        {
            logger.LogError("LeagueId is not set in LeagueProvider");
            return;
        }

        // get standings data -> use handlers to get easy data access
        var seasonId = data as long? ?? -1;
        IRequest<SeasonModel> seasonRequest = seasonId > 0 ? new GetSeasonRequest(seasonId) : new GetCurrentSeasonRequest();
        var season = await mediator.Send(seasonRequest, cancellationToken);
        if (season is null)
        {
            if (seasonId > 0)
            {
                logger.LogError("No season found for league {LeagueId} - {SeasonId}", dbContext.LeagueProvider.LeagueId, seasonId);
            }
            else
            {
                logger.LogError("No current season found for league {LeagueId}", dbContext.LeagueProvider.LeagueId);
            }
            return;
        }
        var standings = await mediator.Send(new GetStandingsFromSeasonRequest(season.SeasonId), cancellationToken);
        if (standings is null || !standings.Any())
        {
            logger.LogError("No standings found for season {SeasonId}", season.SeasonId);
            return;
        }

        var embeds = new List<object>();
        var driverChampionship = standings.FirstOrDefault();
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
            logger.LogError("No driver championship result found for season {SeasonId}", season.SeasonId);
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .SetTitle($"Standings")
            .SetDescription($"Latest standings")
            .SetColor(3066993)
            .SetTimestamp(DateTime.UtcNow);

        embed.AddTableFieldsWithSplitting(driverChampionship.Name, CreateStandingsTable(driverChampionship));

        embed.AddField("", $"Full standings at: https://irleaguemanager.net/{leagueName}/Standings/Seasons/{season.SeasonId}");

        embed.SetFooter("Data provided by irleaguemanager.net");

        embeds.Add(embed.Build());

        await SendMessageWithEmbeds(url, embeds, cancellationToken);
    }

    static DiscordEmbedTable CreateStandingsTable(StandingsModel standings)
    {
        var table = new DiscordEmbedTable();
        var isTeamStanding = standings.IsTeamStanding;
        table.AddColumn(3, "Pos", standings.StandingRows, r => r.Position.ToString());
        if (isTeamStanding)
        {
            table.AddColumn(24, "Team", standings.StandingRows, r => r.TeamName ?? "");
        }
        else
        {
            table.AddColumn(24, "Driver", standings.StandingRows, r => $"{r.Firstname} {r.Lastname}");
        }
        table.AddColumn(6, "Points", standings.StandingRows, r => r.TotalPoints.ToString());
        return table;
    }
}
