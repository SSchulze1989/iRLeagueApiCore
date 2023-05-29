using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Championships;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Championships;
public sealed class PutChampSeasonHandlerTests : ChampionshipHandlersTestsBase<PutChampSeasonHandler, PutChampSeasonRequest, ChampSeasonModel>
{
    protected override PutChampSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutChampSeasonRequest> validator)
    {
        return new(logger, dbContext, new[] { validator }, LeagueProvider);
    }

    protected override PutChampSeasonRequest DefaultRequest()
    {
        throw new NotImplementedException();
    }

    //private PutChampSeasonRequest DefaultRequest(long leagueId, long champSeasonId)
    //{
    //    var resultConfigId = dbContext.StandingConfigurations.First().StandingConfigId;
    //    var model = fixture.Build<PutChampSeasonModel>()
    //        .With(x => x.ResultConfigs, fixture.Build<ResultConfigInfoModel>()
    //            .With(x => x.ResultConfigId, resultConfigId).CreateMany(1).ToList());
    //    return new(leagueId, champSeasonId, model);
    //}
}
