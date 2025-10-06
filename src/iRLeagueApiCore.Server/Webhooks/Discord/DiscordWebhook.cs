using iRLeagueApiCore.Common.Models;
using System.Security.Policy;
using System.Threading;

namespace iRLeagueApiCore.Server.Webhooks.Discord;

public abstract class DiscordWebhook : ILeagueWebhook
{
    protected readonly ILogger<DiscordWebhook> logger;
    protected readonly LeagueDbContext dbContext;
    protected readonly IMediator mediator;
    protected readonly HttpClient httpClient;

    public DiscordWebhook(ILogger<DiscordWebhook> logger, LeagueDbContext dbContext, IMediator mediator, HttpClient httpClient)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.mediator = mediator;
        this.httpClient = httpClient;
    }

    public abstract Task SendAsync(object? data, string url, CancellationToken cancellationToken = default);

    static protected string IntervalToString(Interval interval)
    {
        if (interval.Laps == 0)
        {
            return $"{DateTime.Today.Add(interval.Time):mm:ss.fff}";
        }
        return $"{interval.Laps}L";
    }

    static protected string LapTimeToString(TimeSpan lapTime)
    {
        if (lapTime > TimeSpan.Zero)
        {
            return $"{DateTime.Today.Add(lapTime):mm:ss.fff}";
        }
        return $"--:--.---";
    }

    protected async Task SendMessageWithEmbeds(string url, List<object> embeds, CancellationToken cancellationToken)
    {
        // Create Discord message
        var message = new Dictionary<string, object>()
        {
            ["embeds"] = embeds,
        };

        // Send Message to Discord webhook
        var content = JsonContent.Create(message);

        logger.LogDebug("Sending Discord webhook to {Url} with content: {Content}", url, await content.ReadAsStringAsync(cancellationToken));
        var response = await httpClient.PostAsync(url, content, cancellationToken);
        logger.LogDebug("Discord webhook response: {StatusCode} - {Response}", response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
