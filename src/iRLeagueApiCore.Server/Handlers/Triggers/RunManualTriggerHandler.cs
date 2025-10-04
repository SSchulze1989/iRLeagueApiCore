
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Services.TriggerService;

namespace iRLeagueApiCore.Server.Handlers.Triggers;

public record RunManualTriggerRequest(long TriggerId, TriggerParameterModel TriggerParameters) : IRequest;

public class RunManualTriggerHandler : IRequestHandler<RunManualTriggerRequest>
{
    private readonly LeagueDbContext dbContext;
    private readonly TriggerHostedService triggerService;

    public RunManualTriggerHandler(LeagueDbContext dbContext, TriggerHostedService triggerService)
    {
        this.dbContext = dbContext;
        this.triggerService = triggerService;
    }

    async Task IRequestHandler<RunManualTriggerRequest>.Handle(RunManualTriggerRequest request, CancellationToken cancellationToken)
    {
        await triggerService.ProcessManualTrigger(dbContext, request.TriggerId, request.TriggerParameters, cancellationToken);
    }
}
