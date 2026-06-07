using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Services.DriverPoints;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints;

public record GetDriverPointsForMemberRequest(long MemberId) : IRequest<IEnumerable<DriverPointsAccountModel>>;

public sealed class GetDriverPointsForMemberHandler : HandlerBase<GetDriverPointsForMemberHandler, GetDriverPointsForMemberRequest, IEnumerable<DriverPointsAccountModel>>
{
    private readonly IDriverPointsService service;

    public GetDriverPointsForMemberHandler(ILogger<GetDriverPointsForMemberHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetDriverPointsForMemberRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<IEnumerable<DriverPointsAccountModel>> Handle(GetDriverPointsForMemberRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await GetCurrentLeagueEntityAsync(cancellationToken) ?? throw new ResourceNotFoundException("League not found");
        var accounts = await service.GetAccountsForMemberAsync(league.Id, request.MemberId);
        return accounts;
    }
}
