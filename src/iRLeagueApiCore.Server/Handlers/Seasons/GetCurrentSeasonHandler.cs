﻿using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Seasons
{
    public record GetCurrentSeasonRequest(long LeagueId) : IRequest<SeasonModel>;

    public class GetCurrentSeasonHandler : SeasonHandlerBase<GetCurrentSeasonHandler, GetCurrentSeasonRequest>,
        IRequestHandler<GetCurrentSeasonRequest, SeasonModel>
    {
        public GetCurrentSeasonHandler(ILogger<GetCurrentSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetCurrentSeasonRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<SeasonModel> Handle(GetCurrentSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSeasonId = await dbContext.Events
                .Where(x => x.LeagueId == request.LeagueId)
                .Where(x => x.Date < DateTime.UtcNow)
                .OrderByDescending(x => x.Date)
                .Select(x => (long?)x.Schedule.SeasonId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ResourceNotFoundException();
            var getSeason = await MapToGetSeasonModel(request.LeagueId, getSeasonId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getSeason;
        }
    }
}
