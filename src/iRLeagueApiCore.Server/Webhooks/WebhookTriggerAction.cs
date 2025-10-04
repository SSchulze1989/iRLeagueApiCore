using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Services.TriggerService.Actions;

namespace iRLeagueApiCore.Server.Webhooks;
public sealed class WebhookTriggerAction : ITriggerAction
{
    private readonly ILogger<WebhookTriggerAction> logger;
    private readonly IServiceProvider serviceProvider;

    public WebhookTriggerAction(ILogger<WebhookTriggerAction> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(TriggerParameterModel triggerParameter, Dictionary<string, object> actionParameters, CancellationToken cancellationToken = default)
    {
        // get webhook type from trigger.ActionParameters
        object? webhookTypeData = actionParameters.GetValueOrDefault("Type")?.ToString();
        WebhookType webhookTypeEnum = webhookTypeData switch
        {
            string => Enum.Parse<WebhookType>((string)webhookTypeData),
            WebhookType => (WebhookType)webhookTypeData,
            _ => throw new InvalidOperationException("Invalid webhook type in trigger action parameters"),
        };
        Type webhookType = webhookTypeEnum switch
        {
            WebhookType.EventResult => typeof(IEventResultWebhook),
            _ => throw new NotSupportedException($"Webhook type {webhookTypeEnum} is not supported"),
        };

        // get webhook url from trigger.ActionParameters
        object? urlData = actionParameters.GetValueOrDefault("Url")?.ToString();
        if (urlData is not string url || string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException("Invalid webhook url in trigger action parameters");
        }

        // get client type from trigger.ActionParameters
        object? clientTypeData = actionParameters.GetValueOrDefault("Client")?.ToString();
        WebhookClientType clientType = clientTypeData switch
        {
            string => Enum.Parse<WebhookClientType>((string)clientTypeData),
            WebhookClientType => (WebhookClientType)clientTypeData,
            _ => throw new InvalidOperationException("Invalid webhook client type in trigger action parameters"),
        };

        // get webhook data
        object? data = webhookTypeEnum switch
        {
            WebhookType.EventResult => triggerParameter.RefId1.GetValueOrDefault(),
            _ => throw new NotSupportedException($"Webhook type {webhookTypeEnum} is not supported"),
        };

        logger.LogInformation("Executing WebhookTriggerAction with parameters: {Parameters}", actionParameters);
        
        // send webhook
        var webhook = (ILeagueWebhook)serviceProvider.GetRequiredKeyedService(webhookType, clientType);
        await webhook.SendAsync(data, url, cancellationToken);
    }
}
