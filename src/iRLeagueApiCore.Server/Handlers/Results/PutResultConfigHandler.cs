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

namespace iRLeagueApiCore.Server.Handlers.Results
{
    public record PutResultConfigRequest(long LeagueId, long ResultConfigId, LeagueUser User, PutResultConfigModel Model) : IRequest<ResultConfigModel>;

    public class PutResultConfigHandler : ResultConfigHandlerBase<PutResultConfigHandler, PutResultConfigRequest>, 
        IRequestHandler<PutResultConfigRequest, ResultConfigModel>
    {
        public PutResultConfigHandler(ILogger<PutResultConfigHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutResultConfigRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<ResultConfigModel> Handle(PutResultConfigRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putResultConfig = await GetResultConfigEntity(request.LeagueId, request.ResultConfigId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            putResultConfig = await MapToResultConfigEntityAsync(request.User, request.Model, putResultConfig, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getResultConfig = await MapToResultConfigModel(putResultConfig.LeagueId, putResultConfig.ResultConfigId, cancellationToken);
            return getResultConfig;
        }
    }
}