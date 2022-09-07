using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record GetPointRuleRequest(long LeagueId, long PointRuleId) : IRequest<PointRuleModel>;

    public class GetPointRuleHandler : PointRuleHandlerBase<GetPointRuleHandler, GetPointRuleRequest>, 
        IRequestHandler<GetPointRuleRequest, PointRuleModel>
    {
        public GetPointRuleHandler(ILogger<GetPointRuleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetPointRuleRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<PointRuleModel> Handle(GetPointRuleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getPointRule = await MapToPointRuleModel(request.LeagueId, request.PointRuleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getPointRule;
        }
    }
}
