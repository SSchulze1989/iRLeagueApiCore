
namespace iRLeagueApiCore.Services.TriggerService.Actions;
public sealed class WebhookTriggerAction : ITriggerAction
{
    private readonly ILogger<WebhookTriggerAction> logger;

    public WebhookTriggerAction(ILogger<WebhookTriggerAction> logger)
    {
        this.logger = logger;
    }

    public Task ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing WebhookTriggerAction with parameters: {Parameters}", parameters);
        return Task.CompletedTask;
    }
}
