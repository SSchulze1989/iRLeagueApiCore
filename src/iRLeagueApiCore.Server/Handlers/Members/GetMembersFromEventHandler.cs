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
            var @event = await dbContext.Events
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .Select(x => new {
                    x.LeagueId,
                    x.EventId,
                    EventResult = new
                    {
                        SessionResults = x.EventResult.SessionResults.Select(y => new
                        {
                            ResultRows = y.ResultRows.Select(z => new
                            {
                                MemberId = z.Member.Id
                            })
                        })
                    },
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ResourceNotFoundException();
            if (@event.EventResult == null)
            {
                return Array.Empty<MemberInfoModel>();
            }
            var memberIds = new List<long>();
            foreach(var sessionResult in @event.EventResult.SessionResults)
            {
                foreach(var memberId in sessionResult.ResultRows.Select(x => x.MemberId))
                {
                    memberIds.Add(memberId);
                }
            }
            
            return await MapToMemberInfoListAsync(memberIds, cancellationToken);
        }
    }
}
