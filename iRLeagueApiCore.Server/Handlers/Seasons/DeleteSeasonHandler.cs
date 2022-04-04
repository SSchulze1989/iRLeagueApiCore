using FluentValidation;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Seasons
{
    public record DeleteSeasonRequest(long LeagueId, long SeasonId) : IRequest;

    public class DeleteSeasonHandler : SeasonHandlerBase<DeleteSeasonHandler, DeleteSeasonRequest>, IRequestHandler<DeleteSeasonRequest>
    {
        public DeleteSeasonHandler(ILogger<DeleteSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteSeasonRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public Task<Unit> Handle(DeleteSeasonRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
