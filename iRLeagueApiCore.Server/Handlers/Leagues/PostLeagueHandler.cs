using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Leagues
{
    public record PostLeagueRequest(LeagueUser User, PostLeagueModel Model) : IRequest<GetLeagueModel>;

    public class PostLeagueHandler : IRequestHandler<PostLeagueRequest, GetLeagueModel>
    {
        private readonly ILogger<PostLeagueHandler> _logger;
        private readonly LeagueDbContext dbContext;
        private readonly IEnumerable<IValidator<PostLeagueRequest>> validators;

        public PostLeagueHandler(ILogger<PostLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostLeagueRequest>> validators)
        {
            _logger = logger;
            this.dbContext = dbContext;
            this.validators = validators;
        }

        public async Task<GetLeagueModel> Handle(PostLeagueRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            _logger.LogInformation("Create league {LeagueName}", request.Model.Name);
            var leagueEntity = MapToLeagueEntity(request.User, request.Model, new LeagueEntity());
            dbContext.Leagues.Add(leagueEntity);
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("League {LeagueName} successfully created", request.Model.Name);
            var getLeague = await MapToGetLeagueModel(leagueEntity.Id) ?? throw new ResourceNotFoundException("Created resource not found!");
            return getLeague;
        }

        protected virtual LeagueEntity MapToLeagueEntity(LeagueUser user, PostLeagueModel postLeague, LeagueEntity leagueEntity)
        {
            var createdOn = DateTime.UtcNow;
            leagueEntity.Name = postLeague.Name;
            leagueEntity.NameFull = postLeague.NameFull;
            leagueEntity.CreatedOn = createdOn;
            leagueEntity.LastModifiedOn = createdOn;
            leagueEntity.CreatedByUserId = user.Id;
            leagueEntity.LastModifiedByUserId = user.Id;
            leagueEntity.CreatedByUserName = user.Name;
            leagueEntity.LastModifiedByUserName = user.Name;
            return leagueEntity;
        }

        protected virtual async Task<GetLeagueModel> MapToGetLeagueModel(long leagueId)
        {
            return await dbContext.Leagues
                .Where(x => x.Id == leagueId)
                .Select(x => new GetLeagueModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    NameFull = x.NameFull,
                    SeasonIds = x.Seasons
                        .Select(season => season.SeasonId)
                        .ToList(),
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedByUserName = x.CreatedByUserName,
                    CreatedOn = x.CreatedOn,
                    LastModifiedByUserId = x.LastModifiedByUserId,
                    LastModifiedByUserName = x.LastModifiedByUserName,
                    LastModifiedOn = x.LastModifiedOn,
                })
                .SingleOrDefaultAsync();
        }
    }
}
