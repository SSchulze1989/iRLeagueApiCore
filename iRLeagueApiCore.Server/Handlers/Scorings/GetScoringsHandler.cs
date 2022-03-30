using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record GetScoringRequest(long LeagueId, long ScoringId) : IRequest<GetScoringModel>;

    public class GetScoringHandler : ScoringHandlerBase, IRequestHandler<GetScoringRequest, GetScoringModel>
    {
        private readonly IEnumerable<IValidator<GetScoringRequest>> validators;

        public GetScoringHandler(LeagueDbContext dbContext, IEnumerable<IValidator<GetScoringRequest>> validators)
            : base(dbContext)
        {
            this.validators = validators;
        }

        public async Task<GetScoringModel> Handle(GetScoringRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            return await MapToGetScoringModelAsync(request.LeagueId, request.ScoringId) ?? throw new ResourceNotFoundException();
        }
    }
}
