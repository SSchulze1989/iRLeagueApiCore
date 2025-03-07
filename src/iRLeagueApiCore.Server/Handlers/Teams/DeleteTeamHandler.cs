﻿namespace iRLeagueApiCore.Server.Handlers.Teams;

public record DeleteTeamRequest(long TeamId) : IRequest<Unit>;

public class DeleteTeamHandler : TeamsHandlerBase<DeleteTeamHandler, DeleteTeamRequest, Unit>
{
    public DeleteTeamHandler(ILogger<DeleteTeamHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteTeamRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteTeamRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deleteTeam = await GetTeamEntity(request.TeamId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.Teams.Remove(deleteTeam);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
