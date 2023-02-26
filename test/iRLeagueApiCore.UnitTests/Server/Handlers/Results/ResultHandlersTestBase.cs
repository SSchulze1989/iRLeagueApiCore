using MediatR;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;
public abstract class ResultHandlersTestsBase<THandler, TRequest, TResult> : 
    HandlersTestsBase<THandler, TRequest, TResult>
    where THandler : IRequestHandler<TRequest, TResult>
    where TRequest : class, IRequest<TResult>
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        // Create results
        var @event = dbContext.Events.First();
        var resultConfig = accessMockHelper.CreateConfiguration(@event);
        dbContext.ResultConfigurations.Add(resultConfig);
        var result = accessMockHelper.CreateScoredResult(@event, resultConfig);
        dbContext.ScoredEventResults.Add(result);

        await dbContext.SaveChangesAsync();
    }
}
