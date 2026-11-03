using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Mocking.DataAccess;
using iRLeagueApiCore.Server.Handlers.Events;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Events;

public sealed class GetEventHasResultTests : DataAccessTestsBase
{
    [Fact]
    public async Task ShouldSetHasResult_WhenOnlyUploadedResultExists()
    {
        var @event = await dbContext.Events.FirstAsync();
        var scoredEventResults = @event.ScoredEventResults;
        foreach (var scoredEventResult in scoredEventResults)
        {
            dbContext.ScoredEventResults.Remove(scoredEventResult);
        }
        await dbContext.SaveChangesAsync();

        var handler = new GetEventHandler(Mock.Of<ILogger<GetEventHandler>>(), dbContext, []);

        var result = await handler.Handle(new GetEventRequest(@event.EventId, false), default);

        result.HasResult.Should().BeTrue();
    }
}
