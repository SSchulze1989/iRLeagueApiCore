﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record PutPointRuleRequest(long LeagueId, long PointRuleId, LeagueUser User, PutPointRuleModel Model) : IRequest<PointRuleModel>;

    public class PutPointRuleHandler : PointRuleHandlerBase<PutPointRuleHandler, PutPointRuleRequest>, 
        IRequestHandler<PutPointRuleRequest, PointRuleModel>
    {
        public PutPointRuleHandler(ILogger<PutPointRuleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutPointRuleRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<PointRuleModel> Handle(PutPointRuleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putPointRule = await GetPointRuleEntityAsync(request.LeagueId, request.PointRuleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            putPointRule = await MapToPointRuleEntityAsync(request.User, request.Model, putPointRule, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getPointRule = await MapToPointRuleModel(request.LeagueId, putPointRule.PointRuleId, cancellationToken);
            return getPointRule;
        }
    }
}