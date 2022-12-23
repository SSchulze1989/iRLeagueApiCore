using iRLeagueApiCore.Common.Models.Members;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Members
{
    public class MembersHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public MembersHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        protected async Task<IEnumerable<MemberInfoModel>> MapToMemberInfoListAsync(IEnumerable<long> memberIds, CancellationToken cancellationToken)
        {
            return await dbContext.Members
                .Where(x => memberIds.Contains(x.Id))
                .Select(MapToMemberInfoExpression)
                .ToListAsync(cancellationToken);
        }

        protected Expression<Func<MemberEntity, MemberInfoModel>> MapToMemberInfoExpression => member => new MemberInfoModel()
        {
            MemberId = member.Id,
            FirstName = member.Firstname,
            LastName = member.Lastname,
        };
    }
}
