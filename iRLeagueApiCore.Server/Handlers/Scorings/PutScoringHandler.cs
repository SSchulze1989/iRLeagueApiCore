﻿using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record PutScoringRequest(long LeagueId, long ScoringId, PutScoringModel Model) : IRequest<GetScoringModel>;

    public class PutScoringHandler : ScoringHandlerBase, IRequestHandler<PutScoringRequest, GetScoringModel>
    {
        private readonly ILogger<PutScoringHandler> _logger;
        private readonly IEnumerable<IValidator<PutScoringRequest>> validators;

        public PutScoringHandler(ILogger<PutScoringHandler> logger, LeagueDbContext dbContext, 
            IEnumerable<IValidator<PutScoringRequest>> validators) : base(dbContext)
        {
            _logger = logger;
            this.validators = validators;
        }

        public async Task<GetScoringModel> Handle(PutScoringRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putScoring = await GetScoringEntityAsync(request.LeagueId, request.ScoringId) ?? throw new ResourceNotFoundException();
            await MapToScoringEntityAsync(request.LeagueId, request.Model, putScoring);
            await dbContext.SaveChangesAsync();
            var getScoring = await MapToGetScoringModelAsync(request.LeagueId, putScoring.ScoringId);
            return getScoring;
        }
    }
}
