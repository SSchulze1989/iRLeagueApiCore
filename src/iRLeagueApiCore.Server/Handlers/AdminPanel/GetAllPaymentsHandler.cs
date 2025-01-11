using iRLeagueApiCore.Server.Models.Payments;

namespace iRLeagueApiCore.Server.Handlers.AdminPanel;

public record GetAllPaymentsRequest(long? LeagueId = null) : IRequest<IEnumerable<PaymentModel>>;

public class GetAllPaymentsHandler : AdminHandlerBase<GetAllPaymentsHandler, GetAllPaymentsRequest, IEnumerable<PaymentModel>>,
    IRequestHandler<GetAllPaymentsRequest, IEnumerable<PaymentModel>>
{
    public GetAllPaymentsHandler(ILogger<GetAllPaymentsHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetAllPaymentsRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<PaymentModel>> Handle(GetAllPaymentsRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getSubscriptions = await dbContext.Payments
            .Where(x => request.LeagueId == null || x.LeagueId == request.LeagueId)
            .Select(MapToPaymentModelExpression)
            .ToListAsync(cancellationToken);
        return getSubscriptions;
    }
}
