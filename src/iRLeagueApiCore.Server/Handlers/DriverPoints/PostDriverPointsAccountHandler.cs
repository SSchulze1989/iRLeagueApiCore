using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Services.DriverPoints;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints;

public record PostDriverPointsAccountRequest(LeagueUser User, CreateDriverPointsAccountModel Model) : IRequest<DriverPointsAccountModel>;

public sealed class PostDriverPointsAccountHandler : HandlerBase<PostDriverPointsAccountHandler, PostDriverPointsAccountRequest, DriverPointsAccountModel>
{
    private readonly IDriverPointsService service;

    public PostDriverPointsAccountHandler(ILogger<PostDriverPointsAccountHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostDriverPointsAccountRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<DriverPointsAccountModel> Handle(PostDriverPointsAccountRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await GetCurrentLeagueEntityAsync(cancellationToken) ?? throw new ResourceNotFoundException("League not found");
        var createdById = long.TryParse(request.User.Id, out var uid) ? uid : 0;
        var result = await service.CreateAccountAsync(league.Id, request.Model, createdById);
        return result ?? throw new ResourceNotFoundException("Account creation failed");
    }
}
