namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record DeleteVoteCategoryRequest(long CatId) : IRequest<Unit>;

public class DeleteVoteCategoryHandler : VoteCategoriesHandlerBase<DeleteVoteCategoryHandler, DeleteVoteCategoryRequest, Unit>
{
    public DeleteVoteCategoryHandler(ILogger<DeleteVoteCategoryHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteVoteCategoryRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteVoteCategoryRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deleteVoteCategory = await GetVoteCategoryEntityAsync(request.CatId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.Remove(deleteVoteCategory);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
