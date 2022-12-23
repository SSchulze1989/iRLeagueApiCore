namespace iRLeagueApiCore.Server.Handlers.Results
{
    public record DeleteResultConfigRequest(long LeagueId, long ResultConfigId) : IRequest;

    public class DeleteResultConfigHandler : ResultConfigHandlerBase<DeleteResultConfigHandler, DeleteResultConfigRequest>,
        IRequestHandler<DeleteResultConfigRequest>
    {
        public DeleteResultConfigHandler(ILogger<DeleteResultConfigHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteResultConfigRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeleteResultConfigRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var deleteResultConfig = await GetResultConfigEntity(request.LeagueId, request.ResultConfigId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            dbContext.ResultConfigurations.Remove(deleteResultConfig);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
