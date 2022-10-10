using FluentValidation;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Members
{
    public record GetMembersFromEventRequest(long LeagueId, long EventId) : IRequest<IEnumerable<MemberInfoModel>>;

    public class GetMembersFromEventHandler : MembersHandlerBase<GetMembersFromEventHandler, GetMembersFromEventRequest>,
        IRequestHandler<GetMembersFromEventRequest, IEnumerable<MemberInfoModel>>
    {
        public GetMembersFromEventHandler(ILogger<GetMembersFromEventHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetMembersFromEventRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<MemberInfoModel>> Handle(GetMembersFromEventRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getMembers = await GetMembersFromEvent(request.LeagueId, request.EventId, cancellationToken);
            return getMembers;
        }

        protected async Task<IEnumerable<MemberInfoModel>> GetMembersFromEvent(long leagueId, long eventId, CancellationToken cancellationToken)
        {
            var resultMembers = await dbContext.EventResults
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .Select(x => x.SessionResults
                        .SelectMany(y => y.ResultRows)
                        .Select(y => y.MemberId))
                .SelectMany(x => x!)
                .ToListAsync(cancellationToken)
                ?? throw new ResourceNotFoundException();
            var scoredResultMembers = await dbContext.ScoredEventResults
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .Select(x => x.ScoredSessionResults
                    .SelectMany(y => y.ScoredResultRows)
                    .Where(y => y.MemberId != null)
                    .Select(y => y.MemberId.GetValueOrDefault()))
                .SelectMany(x => x)
                .ToListAsync(cancellationToken);
            var memberIds = resultMembers
                .Concat(scoredResultMembers)
                .Distinct();
            
            return await MapToMemberInfoListAsync(memberIds, cancellationToken);
        }
    }
}
