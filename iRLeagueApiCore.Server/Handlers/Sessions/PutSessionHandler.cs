using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Sessions
{
    public record PutSessionRequest(long LeagueId, LeagueUser User, long SessionId, PutSessionModel Model) : IRequest<GetSessionModel>;

    public class PutSessionHandler : SessionHandlerBase<PutSessionHandler, PutSessionRequest>, IRequestHandler<PutSessionRequest, GetSessionModel>
    {
        public PutSessionHandler(ILogger<PutSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutSessionRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<GetSessionModel> Handle(PutSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putSession = await GetSessionEntityAsync(request.LeagueId, request.SessionId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            await MapToSessionEntityAsync(request.LeagueId, request.User, request.Model, putSession, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getSession = await MapToGetSessionModelAsync(request.LeagueId, request.SessionId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getSession;
        }
    }
}
