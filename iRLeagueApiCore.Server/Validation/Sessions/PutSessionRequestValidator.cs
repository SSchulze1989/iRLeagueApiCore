using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Validation.Sessions
{
    public class PutSessionRequestValidator : AbstractValidator<PutSessionRequest>
    {
        private readonly LeagueDbContext dbContext;

        public PutSessionRequestValidator(LeagueDbContext dbContext)
        {
            this.dbContext = dbContext;

            RuleFor(x => x.Model.SubSessions)
                // must have property SubSessions
                .NotNull()
                .WithMessage("SubSessions required");
            RuleForEach(x => x.Model.SubSessions)
                .MustAsync(SubSessionIdValid)
                .WithMessage($"{nameof(SubSessionModel.SubSessionId)} does not exist. " +
                    $"Use \"0\" for new subsession or make sure id targets an existing subsession that is part of the session");
        }

        public async Task<bool> SubSessionIdValid(PutSessionRequest request, PutSessionSubSessionModel subSession, CancellationToken cancellationToken)
        {
            var subSessionId = subSession.SubSessionId;
            return subSessionId == 0 || 
                await dbContext.SubSessions
                .Where(x => x.SessionId == request.SessionId)
                .Where(x => x.SubSessionId == subSession.SubSessionId)
                .AnyAsync();
        }
    }
}
