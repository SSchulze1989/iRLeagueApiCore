﻿using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Leagues;

public sealed record GetLeagueByNameRequest(string LeagueName) : IRequest<LeagueModel>;

public sealed class GetLeagueByNameHandler : LeagueHandlerBase<GetLeagueByNameHandler, GetLeagueByNameRequest>,
    IRequestHandler<GetLeagueByNameRequest, LeagueModel>
{
    public GetLeagueByNameHandler(ILogger<GetLeagueByNameHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetLeagueByNameRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<LeagueModel> Handle(GetLeagueByNameRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getLeague = await MapToLeagueModelFromName(request.LeagueName, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getLeague;
    }

    private async Task<LeagueModel?> MapToLeagueModelFromName(string leagueName, CancellationToken cancellationToken)
    {
        return await dbContext.Leagues
            .Where(x => x.Name == leagueName)
            .Select(MapToGetLeagueModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
