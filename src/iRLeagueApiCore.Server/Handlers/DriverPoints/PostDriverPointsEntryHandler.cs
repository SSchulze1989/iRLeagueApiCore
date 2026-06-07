using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Services.DriverPoints;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints;

public record PostDriverPointsEntryRequest(long AccountId, CreateDriverPointsEntryModel Model) : IRequest<DriverPointsEntryModel>;

public sealed class PostDriverPointsEntryHandler : HandlerBase<PostDriverPointsEntryHandler, PostDriverPointsEntryRequest, DriverPointsEntryModel>
{
    private readonly IDriverPointsService service;

    public PostDriverPointsEntryHandler(ILogger<PostDriverPointsEntryHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostDriverPointsEntryRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<DriverPointsEntryModel> Handle(PostDriverPointsEntryRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await GetCurrentLeagueEntityAsync(cancellationToken) ?? throw new ResourceNotFoundException("League not found");
        var entry = await service.AddEntryAsync(league.Id, request.AccountId, request.Model);
        return entry;
    }
}
