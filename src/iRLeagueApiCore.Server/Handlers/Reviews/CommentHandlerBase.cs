﻿using FluentValidation;
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
    public class CommentHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public CommentHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        protected virtual async Task<ReviewCommentEntity> GetCommentEntityAsync(long leagueId, long commentId, CancellationToken cancellationToken)
        {
            return await dbContext.ReviewComments
                .Include(x => x.ReviewCommentVotes)
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.CommentId == commentId)
                .FirstOrDefaultAsync();
        }

        protected virtual async Task<ReviewCommentEntity> MapToReviewCommentEntityAsync(LeagueUser user, PostReviewCommentModel postComment, 
            ReviewCommentEntity commentEntity, CancellationToken cancellationToken)
        {
            commentEntity.Text = postComment.Text;
            commentEntity.ReviewCommentVotes = await MapCommentVoteList(postComment.Votes, commentEntity.ReviewCommentVotes, cancellationToken);
            return UpdateVersionEntity(user, commentEntity);
        }

        protected virtual async Task<ReviewCommentEntity> MapToReviewCommentEntityAsync(LeagueUser user, PutReviewCommentModel putComment,
            ReviewCommentEntity commentEntity, CancellationToken cancellationToken)
        {
            return await MapToReviewCommentEntityAsync(user, (PostReviewCommentModel)putComment, commentEntity, cancellationToken);
        }

        protected virtual async Task<ReviewCommentVoteEntity> MapToCommentVoteEntityAsync(CommentVoteModel voteModel, ReviewCommentVoteEntity voteEntity,
            CancellationToken cancellationToken)
        {
            voteEntity.Description = voteModel.Description;
            voteEntity.MemberAtFault = await GetMemberEntityAsync(voteModel.MemberAtFault.MemberId, cancellationToken)
                ?? throw new HandlerOperationException("Error while trying to update vote entity: MemberAtFault was null");
            voteEntity.VoteCategory = await GetVoteCategoryEntityAsync(voteEntity.LeagueId, voteModel.VoteCategoryId);
            return voteEntity;
        }

        protected virtual async Task<VoteCategoryEntity> GetVoteCategoryEntityAsync(long leagueId, long voteCategoryId)
        {
            return await dbContext.VoteCategorys
                .FirstOrDefaultAsync(x => x.CatId == voteCategoryId);
        }

        protected virtual async Task<ICollection<ReviewCommentVoteEntity>> MapCommentVoteList(IEnumerable<CommentVoteModel> voteModels,
            ICollection<ReviewCommentVoteEntity> voteEntities, CancellationToken cancellationToken)
        {
            // Map votes
            foreach(var voteModel in voteModels)
            {
                var voteEntity = voteEntities
                    .FirstOrDefault(x => x.ReviewVoteId == voteModel.Id);
                if (voteEntity == null)
                {
                    voteEntity = new ReviewCommentVoteEntity();
                    voteEntities.Add(voteEntity);
                }
                await MapToCommentVoteEntityAsync(voteModel, voteEntity, cancellationToken);
            }
            // Delete votes that are no longer in source collection
            var deleteVotes = voteEntities
                .Where(x => voteModels.Any(y => y.Id == x.ReviewVoteId) == false);
            foreach(var deleteVote in deleteVotes)
            {
                dbContext.Remove(deleteVote);
            }
            return voteEntities;
        }

        protected virtual async Task<ReviewCommentModel> MapToReviewCommentModelAsync(long leagueId, long commentId, CancellationToken cancellationToken)
        {
            return await dbContext.ReviewComments
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.CommentId == commentId)
                .Select(MapToReviewCommentModelExpression)
                .FirstOrDefaultAsync(cancellationToken);
        }

        protected Expression<Func<ReviewCommentEntity, ReviewCommentModel>> MapToReviewCommentModelExpression => comment => new ReviewCommentModel()
        {
            LeagueId = comment.LeagueId,
            CommentId = comment.CommentId,
            AuthorName = comment.AuthorName,
            AuthorUserId = comment.AuthorUserId,
            Date = comment.Date,
            ReviewId = comment.ReviewId.GetValueOrDefault(),
            Text = comment.Text,
            Votes = comment.ReviewCommentVotes.Select(vote => new CommentVoteModel()
            {
                Id = vote.ReviewVoteId,
                Description = vote.Description,
                VoteCategoryId = vote.VoteCategoryId.GetValueOrDefault(),
                MemberAtFault = new MemberInfoModel()
                {
                    MemberId = vote.MemberAtFault.Id,
                    FirstName = vote.MemberAtFault.Firstname,
                    LastName = vote.MemberAtFault.Lastname
                }
            }).ToList(),
            CreatedByUserId = comment.CreatedByUserId,
            CreatedByUserName = comment.CreatedByUserName,
            CreatedOn = comment.CreatedOn,
            LastModifiedByUserId = comment.LastModifiedByUserId,
            LastModifiedByUserName = comment.LastModifiedByUserName,
            LastModifiedOn = comment.LastModifiedOn,
        };
    }
}