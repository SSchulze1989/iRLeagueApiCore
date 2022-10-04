using FluentValidation;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Exceptions;
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

        protected virtual async Task<IncidentReviewEntity?> GetReviewEntity(long leagueId, long reviewId, CancellationToken cancellationToken)
        {
            return await dbContext.IncidentReviews
                .Include(x => x.InvolvedMembers)
                .Include(x => x.AcceptedReviewVotes)
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ReviewId == reviewId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        protected virtual async Task<IncidentReviewEntity> MapToReviewEntity(LeagueUser user, PostReviewModel postModel, IncidentReviewEntity reviewEntity, CancellationToken cancellationToken)
        {
            reviewEntity.AuthorName = user.Name;
            reviewEntity.AuthorUserId = user.Id;
            reviewEntity.Corner = postModel.Corner;
            reviewEntity.OnLap = postModel.OnLap;
            reviewEntity.FullDescription = postModel.FullDescription;
            reviewEntity.IncidentKind = postModel.IncidentKind;
            reviewEntity.IncidentNr = postModel.IncidentNr;
            reviewEntity.TimeStamp = postModel.TimeStamp;
            reviewEntity.InvolvedMembers = await GetMemberListAsync(postModel.InvolvedMembers.Select(x => x.MemberId), cancellationToken);
            reviewEntity.ResultLongText = postModel.ResultText;
            reviewEntity.AcceptedReviewVotes = await MapToAcceptedVoteList(postModel.VoteResults, reviewEntity.AcceptedReviewVotes, cancellationToken);
            return UpdateVersionEntity(user, reviewEntity);
        }

        protected virtual async Task<ICollection<AcceptedReviewVoteEntity>> MapToAcceptedVoteList(IEnumerable<VoteModel> voteModels,
            ICollection<AcceptedReviewVoteEntity> voteEntities, CancellationToken cancellationToken)
        {
            // Map votes
            foreach (var voteModel in voteModels)
            {
                var voteEntity = voteEntities
                    .FirstOrDefault(x => x.ReviewVoteId == voteModel.Id);
                if (voteEntity == null)
                {
                    voteEntity = new AcceptedReviewVoteEntity();
                    voteEntities.Add(voteEntity);
                }
                await MapToAcceptedReviewVoteEntityAsync(voteModel, voteEntity, cancellationToken);
            }
            // Delete votes that are no longer in source collection
            var deleteVotes = voteEntities
                .Where(x => voteModels.Any(y => y.Id == x.ReviewVoteId) == false);
            foreach (var deleteVote in deleteVotes)
            {
                dbContext.Remove(deleteVote);
            }
            return voteEntities;
        }

        protected virtual async Task<AcceptedReviewVoteEntity> MapToAcceptedReviewVoteEntityAsync(VoteModel voteModel, AcceptedReviewVoteEntity voteEntity,
            CancellationToken cancellationToken)
        {
            voteEntity.Description = voteModel.Description;
            voteEntity.MemberAtFault = await GetMemberEntityAsync((voteModel.MemberAtFault?.MemberId).GetValueOrDefault(), cancellationToken);
            voteEntity.VoteCategory = await GetVoteCategoryEntityAsync(voteEntity.LeagueId, voteModel.VoteCategoryId);
            return voteEntity;
        }

        protected virtual async Task<ReviewModel?> MapToReviewModel(long leagueId, long reviewId, CancellationToken cancellationToken)
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
            SessionNr = review.Session.SessionNr,
            SessionName = review.Session.Name,
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
                CommentId = comment.CommentId,
                AuthorName = comment.AuthorName,
                AuthorUserId = comment.AuthorUserId,
                Date = comment.Date,
                LeagueId = comment.LeagueId,
                ReviewId = comment.ReviewId.GetValueOrDefault(),
                Text = comment.Text,
                Votes = comment.ReviewCommentVotes.Select(vote => new VoteModel()
                {
                    Id = vote.ReviewVoteId,
                    VoteCategoryId = vote.VoteCategoryId.GetValueOrDefault(),
                    VoteCategoryText = vote.VoteCategory.Text,
                    Description = vote.Description,
                    MemberAtFault = vote.MemberAtFault != null ? new MemberInfoModel()
                    {
                        MemberId = vote.MemberAtFault.Id,
                        FirstName = vote.MemberAtFault.Firstname,
                        LastName = vote.MemberAtFault.Lastname,
                    } : default,
                }).ToList(),
                CreatedByUserId = comment.CreatedByUserId,
                CreatedByUserName = comment.CreatedByUserName,
                CreatedOn = comment.CreatedOn,
                LastModifiedByUserId = comment.LastModifiedByUserId,
                LastModifiedByUserName = comment.LastModifiedByUserName,
                LastModifiedOn = comment.LastModifiedOn,
            }),
            InvolvedMembers = review.InvolvedMembers.Select(member => new MemberInfoModel()
            {
                MemberId = member.Id,
                FirstName = member.Firstname,
                LastName = member.Lastname,
            }).ToList(),
            ResultText = review.ResultLongText,
            VoteResults = review.AcceptedReviewVotes.Select(vote => new VoteModel()
            {
                Id = vote.ReviewVoteId,
                VoteCategoryId = vote.VoteCategoryId.GetValueOrDefault(),
                VoteCategoryText = vote.VoteCategory.Text,
                Description = vote.Description,
                MemberAtFault = vote.MemberAtFault != null ? new MemberInfoModel()
                {
                    MemberId = vote.MemberAtFault.Id,
                    FirstName = vote.MemberAtFault.Firstname,
                    LastName = vote.MemberAtFault.Lastname,
                } : default,
            }).ToList(),
            TimeStamp = review.TimeStamp,
            CreatedByUserId = review.CreatedByUserId,
            CreatedByUserName = review.CreatedByUserName,
            CreatedOn = review.CreatedOn,
            LastModifiedByUserId = review.LastModifiedByUserId,
            LastModifiedByUserName = review.LastModifiedByUserName,
            LastModifiedOn = review.LastModifiedOn,
        };
    }
}
