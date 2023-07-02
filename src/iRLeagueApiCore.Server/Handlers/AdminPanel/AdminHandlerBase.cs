using iRLeagueApiCore.Server.Models.Payments;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.AdminPanel;

public class AdminHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
{
    public AdminHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    protected static Expression<Func<PaymentEntity, PaymentModel>> MapToPaymentModelExpression => payment =>
        new(payment.Id,
            payment.Type,
            payment.PlanId ?? string.Empty,
            payment.Subscription != null ? payment.Subscription.Name : string.Empty,
            payment.Subscription != null ? payment.Subscription.Interval : default,
            payment.SubscriptionId ?? string.Empty,
            payment.LeagueId,
            payment.UserId,
            payment.LastPaymentReceived,
            payment.NextPaymentDue,
            payment.Status);
}
