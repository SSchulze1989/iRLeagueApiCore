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
    public record GetScoringRequest(long LeagueId, long ScoringId) : IRequest<ScoringModel>;

    public class GetScoringHandler : ScoringHandlerBase<GetScoringHandler, GetScoringRequest>, IRequestHandler<GetScoringRequest, ScoringModel>
    {
        public GetScoringHandler(ILogger<GetScoringHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetScoringRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<ScoringModel> Handle(GetScoringRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            return await MapToGetScoringModelAsync(request.LeagueId, request.ScoringId) ?? throw new ResourceNotFoundException();
        }
    }
}
