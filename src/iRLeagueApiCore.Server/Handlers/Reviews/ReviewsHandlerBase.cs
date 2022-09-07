using FluentValidation;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public class ReviewsHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public ReviewsHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        protected virtual async Task<IncidentReviewEntity> GetReviewEntity(long leagueId, long reviewId, CancellationToken cancellationToken)
        {
            return await dbContext.IncidentReviews
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ReviewId == reviewId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        protected virtual async Task<IncidentReviewEntity> MapToReviewEntity(LeagueUser user, PostReviewModel postModel, IncidentReviewEntity reviewEntity, CancellationToken cancellationToken)
        {
            reviewEntity.AuthorName = user.Name;
            reviewEntity.AuthorUserId = user.Id;
            reviewEntity.Corner = postModel.Corner;
            reviewEntity.FullDescription = postModel.FullDescription;
            reviewEntity.IncidentKind = postModel.IncidentKind;
            reviewEntity.IncidentNr = postModel.IncidentNr;
            reviewEntity.TimeStamp = postModel.TimeStamp;
            reviewEntity.InvolvedMembers = await GetMemberListAsync(postModel.InvolvedMembers.Select(x => x.MemberId), cancellationToken);
            return UpdateVersionEntity(user, reviewEntity);
        }

        protected virtual async Task<ReviewModel> MapToReviewModel(long leagueId, long reviewId, CancellationToken cancellationToken)
        {
            var query = dbContext.IncidentReviews
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ReviewId == reviewId)
                .Select(MapToReviewModelExpression);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        protected Expression<Func<IncidentReviewEntity, ReviewModel>> MapToReviewModelExpression => review => new ReviewModel()
        {
            LeagueId = review.LeagueId,
            ReviewId = review.ReviewId,
            SessionId = review.SessionId,
            EventId = review.Session.EventId,
            SeasonId = review.Session.Event.Schedule.SeasonId,
            AuthorName = review.AuthorName,
            AuthorUserId = review.AuthorUserId,
            Corner = review.Corner,
            FullDescription = review.FullDescription,
            IncidentKind = review.IncidentKind,
            IncidentNr = review.IncidentNr,
            OnLap = review.OnLap,
            ReviewComments = review.Comments.Select(comment => new ReviewCommentModel()
            {
                AuthorName = comment.AuthorName,
                AuthorUserId = comment.AuthorUserId,
                Date = comment.Date,
                LeagueId = comment.LeagueId,
                ReviewId = comment.ReviewId.GetValueOrDefault(),
                Text = comment.Text,
                Votes = comment.ReviewCommentVotes.Select(vote => new CommentVoteModel()
                {
                    MemberAtFault = vote.MemberAtFault != null ? new MemberInfoModel()
                    {
                        MemberId = vote.MemberAtFault.Id,
                        FirstName = vote.MemberAtFault.Firstname,
                        LastName = vote.MemberAtFault.Lastname,
                    } : default,
                }).ToList(),
            }),
            InvolvedMembers = review.InvolvedMembers.Select(member => new MemberInfoModel()
            {
                MemberId = member.Id,
                FirstName = member.Firstname,
                LastName = member.Lastname,
            }).ToList(),
        };
    }
}
