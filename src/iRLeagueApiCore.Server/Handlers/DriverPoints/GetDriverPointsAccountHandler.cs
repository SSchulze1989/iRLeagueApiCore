using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Services.DriverPoints;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints;

public record GetDriverPointsAccountRequest(long AccountId) : IRequest<DriverPointsAccountModel>;

public sealed class GetDriverPointsAccountHandler : HandlerBase<GetDriverPointsAccountHandler, GetDriverPointsAccountRequest, DriverPointsAccountModel>
{
    private readonly IDriverPointsService service;

    public GetDriverPointsAccountHandler(ILogger<GetDriverPointsAccountHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetDriverPointsAccountRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<DriverPointsAccountModel> Handle(GetDriverPointsAccountRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await GetCurrentLeagueEntityAsync(cancellationToken) ?? throw new ResourceNotFoundException("League not found");
        var account = await service.GetAccountAsync(league.Id, request.AccountId);
        if (account == null)
        {
            throw new ResourceNotFoundException($"Driver points account {request.AccountId} not found");
        }
        return account;
    }
}
