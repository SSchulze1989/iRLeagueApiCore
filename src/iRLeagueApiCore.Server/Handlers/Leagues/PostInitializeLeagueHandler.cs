﻿using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Leagues;

public record PostIntitializeLeagueRequest(long LeagueId) : IRequest<LeagueModel>;

public class PostInitializeLeagueHandler : LeagueHandlerBase<PostInitializeLeagueHandler, PostIntitializeLeagueRequest, LeagueModel>
{
    public PostInitializeLeagueHandler(ILogger<PostInitializeLeagueHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<PostIntitializeLeagueRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<LeagueModel> Handle(PostIntitializeLeagueRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await GetLeagueEntityAsync(request.LeagueId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        league.IsInitialized = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        var getLeague = await MapToGetLeagueModelAsync(request.LeagueId, true, cancellationToken)
            ?? throw new InvalidOperationException("Updated resource not found");
        return getLeague;
    }
}
