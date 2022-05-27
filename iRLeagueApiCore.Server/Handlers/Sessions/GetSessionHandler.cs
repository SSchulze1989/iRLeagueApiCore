using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Sessions
{
    public record GetSessionRequest(long LeagueId, long SessionId) : IRequest<SessionModel>;

    public class GetSessionHandler : SessionHandlerBase<GetSessionHandler, GetSessionRequest>, IRequestHandler<GetSessionRequest, SessionModel>
    {
        public GetSessionHandler(ILogger<GetSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSessionRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<SessionModel> Handle(GetSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSession = await MapToGetSessionModelAsync(request.LeagueId, request.SessionId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getSession;
        }
    }
}
