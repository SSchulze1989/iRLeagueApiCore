using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Server.Webhooks;
using iRLeagueApiCore.Server.Webhooks.Discord;
using iRLeagueApiCore.Services.TriggerService.Actions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;
public static class WebhookServiceCollectionExtenstion
{
    public static IServiceCollection AddWebhooks(this IServiceCollection services)
    {
        // Register webhook clients
        services.TryAddKeyedTransient<IEventResultWebhook, DiscordEventResultWebhook>(WebhookClientType.Discord);
        services.TryAddKeyedTransient<IStandingsWebhook, DiscordStandingsWebhook>(WebhookClientType.Discord);

        // Register webhook trigger action
        services.TryAddKeyedTransient<ITriggerAction, WebhookTriggerAction>(TriggerAction.Webhook);
        return services;
    }
}
