using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.EntityFrameworkCore;
using System;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results;

public sealed class DeleteResultHandlerTests : ResultHandlersTestsBase<DeleteResultHandler, DeleteResultRequest, Unit>
{
    protected override DeleteResultHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteResultRequest> validator)
    {
        return new(logger, dbContext, [validator]);
    }

    protected override DeleteResultRequest DefaultRequest()
    {
        return DefaultRequest(TestEventId);
    }

    private DeleteResultRequest DefaultRequest(long eventId)
    {
        return new DeleteResultRequest(eventId);
    }

    protected override void DefaultAssertions(DeleteResultRequest request, Unit result, LeagueDbContext dbContext)
    {
        base.DefaultAssertions(request, result, dbContext);
        var deletedResult = dbContext.ScoredEventResults
            .Where(x => x.EventId == request.EventId)
            .FirstOrDefault();
        deletedResult.Should().BeNull();
    }

    [Fact]
    public async override Task ShouldHandleDefault()
    {
        await base.ShouldHandleDefault();
    }

    [Fact]
    public async override Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }

    [Theory]
    [InlineData(0L, defaultId)]
    [InlineData(defaultId, 0L)]
    [InlineData(-42L, defaultId)]
    [InlineData(defaultId, -42L)]
    public async Task ShouldHandleNotFoundAsync(long? leagueId, long? resultId)
    {
        leagueId ??= TestLeagueId;
        resultId ??= TestEventId;
        accessMockHelper.SetCurrentLeague(leagueId.Value);
        var request = DefaultRequest(resultId.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public async Task ShouldHandle_DeleteResult_WithPenalties()
    {
        using (var dbContext = accessMockHelper.CreateMockDbContext(databaseName))
        {
            // Add penalties to the result
            var result = await dbContext.ScoredEventResults
                .Where(x => x.EventId == TestEventId)
                .FirstAsync();
            foreach (var row in result.ScoredSessionResults.Last().ScoredResultRows.Take(2))
            {
                var penalty = fixture.Build<AddPenaltyEntity>()
                    .With(x => x.ScoredResultRow, row)
                    .Without(x => x.LeagueId)
                    .Create();
                row.AddPenalties.Add(penalty);
            }
            await dbContext.SaveChangesAsync();
        }

        using (var dbContext = accessMockHelper.CreateMockDbContext(databaseName))
        {
            var request = DefaultRequest(TestEventId);
            var handler = CreateTestHandler(dbContext, MockHelpers.TestValidator<DeleteResultRequest>());

            // Pre assertions
            var addedPenalties = await dbContext.AddPenaltys
                .Where(x => x.ScoredResultRow.ScoredSessionResult.ScoredEventResult.EventId == TestEventId)
                .ToListAsync();
            addedPenalties.Should().NotBeEmpty();

            await handler.Handle(request, default);

            // Post assertions
            var deletedPenalties = await dbContext.AddPenaltys
                .Where(x => x.ScoredResultRow.ScoredSessionResult.ScoredEventResult.EventId == TestEventId)
                .ToListAsync();
            deletedPenalties.Should().BeEmpty();
            var deletedResult = await dbContext.ScoredEventResults
                .Where(x => x.EventId == request.EventId)
                .FirstOrDefaultAsync();
            deletedResult.Should().BeNull();            
        }
    }
}
