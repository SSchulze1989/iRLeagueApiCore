using iRLeagueApiCore.Common.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace iRLeagueApiCore.Services.TriggerService.Actions;
internal class TriggerActionProvider
{
    private readonly IServiceProvider serviceProvider;
    public TriggerActionProvider(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
    public ITriggerAction GetTriggerAction(TriggerAction actionType)
    {
        if (serviceProvider.GetKeyedService<ITriggerAction>(actionType) is not ITriggerAction action)
        {
            throw new InvalidOperationException($"Could not create instance of trigger action type {actionType}");
        }
        return action;
    }
}
