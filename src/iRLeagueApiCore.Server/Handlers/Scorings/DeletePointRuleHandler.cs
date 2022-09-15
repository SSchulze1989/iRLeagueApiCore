using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record DeletePointRuleRequest(long LeagueId, long PointRuleId) : IRequest;

    public class DeletePointRuleHandler : PointRuleHandlerBase<DeletePointRuleHandler, DeletePointRuleRequest>, 
        IRequestHandler<DeletePointRuleRequest, Unit>
    {
        public DeletePointRuleHandler(ILogger<DeletePointRuleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeletePointRuleRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeletePointRuleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var deletePointRule = await GetPointRuleEntityAsync(request.LeagueId, request.PointRuleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            dbContext.PointRules.Remove(deletePointRule);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
