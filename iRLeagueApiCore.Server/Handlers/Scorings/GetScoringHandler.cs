using FluentValidation;
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
    public record GetScoringRequest(long LeagueId, long ScoringId) : IRequest<GetScoringModel>;

    public class GetScoringHandler : ScoringHandlerBase<GetScoringHandler, GetScoringRequest>, IRequestHandler<GetScoringRequest, GetScoringModel>
    {
        public GetScoringHandler(ILogger<GetScoringHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetScoringRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<GetScoringModel> Handle(GetScoringRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            return await MapToGetScoringModelAsync(request.LeagueId, request.ScoringId) ?? throw new ResourceNotFoundException();
        }
    }
}
