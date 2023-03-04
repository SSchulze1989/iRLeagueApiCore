using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var @event = await dbContext.Events.FirstAsync();
        var season = await dbContext.Seasons.FirstAsync();
        var championships = await dbContext.Championships.ToListAsync();
        var champSeasons = championships.Select(x => accessMockHelper.CreateChampSeason(x, season)).ToList();
        dbContext.AddRange(champSeasons);
        foreach(var resultConfig in champSeasons.SelectMany(x => x.ResultConfigurations))
        {
            var result = accessMockHelper.CreateScoredResult(@event, resultConfig);
            dbContext.Add(result);
        }

        await dbContext.SaveChangesAsync();
    }
}
