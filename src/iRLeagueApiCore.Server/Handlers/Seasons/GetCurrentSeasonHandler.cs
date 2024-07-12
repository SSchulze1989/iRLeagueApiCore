using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Seasons;

public record GetCurrentSeasonRequest() : IRequest<SeasonModel>;

public sealed class GetCurrentSeasonHandler : SeasonHandlerBase<GetCurrentSeasonHandler,  GetCurrentSeasonRequest, SeasonModel>
{
    public GetCurrentSeasonHandler(ILogger<GetCurrentSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetCurrentSeasonRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<SeasonModel> Handle(GetCurrentSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getSeasonId = await dbContext.Events
            .Where(x => x.Date < DateTime.UtcNow)
            .OrderByDescending(x => x.Date)
            .Select(x => (long?)x.Schedule.SeasonId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var getSeason = await MapToGetSeasonModel(getSeasonId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getSeason;
    }
}
