﻿using Aydsko.iRacingData;
using Aydsko.iRacingData.Member;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Members;

public abstract class MembersHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public MembersHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    protected async Task<LeagueMemberEntity?> GetLeagueMemberEntity(long memberId, CancellationToken cancellationToken)
    {
        return await dbContext.LeagueMembers
            .Include(x => x.Member)
            .Include(x => x.Team)
            .Where(x => x.MemberId == memberId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected async Task<LeagueMemberEntity> MapToLeagueMemberEntity(LeagueMemberEntity entity, PutMemberModel model, CancellationToken cancellationToken)
    {
        entity.Number = model.Number;
        entity.DiscordId = model.DiscordId;
        entity.Team = await dbContext.Teams.FirstOrDefaultAsync(x => x.TeamId == model.TeamId, cancellationToken);
        entity.CountryFlag = model.CountryFlag;
        entity.Profile = model.Profile;
        return entity;
    }

    protected async Task<IEnumerable<MemberModel>> MapToMemberListAsync(IEnumerable<long> memberIds, bool includeProfile = false, CancellationToken cancellationToken = default)
    {
        return await dbContext.LeagueMembers
            .Where(x => memberIds.Contains(x.MemberId))
            .Select(MapToMemberModelExpression(includeProfile: includeProfile))
            .ToListAsync(cancellationToken);
    }

    protected async Task<MemberModel?> MapToMemberModel(long memberId, bool includeProfile = false, CancellationToken cancellationToken = default)
    {
        return await dbContext.LeagueMembers
            .Select(MapToMemberModelExpression(includeProfile))
            .FirstOrDefaultAsync(x => x.MemberId == memberId, cancellationToken);

    }

    protected async Task<MemberProfile?> FetchMemberInfo(string iracingId, IDataClient iRacingDataClient, CancellationToken cancellationToken)
    {
        if (!int.TryParse(iracingId, out var customerId))
        {
            throw new ArgumentException($"Invalid iRacing ID: {iracingId}. It must be a valid integer.", nameof(iracingId));
        }
        var profile = await iRacingDataClient.GetMemberProfileAsync(customerId: customerId, cancellationToken: cancellationToken);
        if (!profile.Data.Success)
        {
            return null;
        }
        return profile.Data;
    }

    protected (string firstName, string lastName) ParseFullName(string fullName)
    {
        var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (nameParts.Length < 2)
        {
            throw new ArgumentException($"Invalid full name: {fullName}. It must contain at least a first and last name.", nameof(fullName));
        }
        var firstName = nameParts[0];
        var lastName = string.Join(' ', nameParts.Skip(1));
        return (firstName, lastName);
    }

    protected Expression<Func<MemberEntity, MemberInfoModel>> MapToMemberInfoExpression => member => new()
    {
        MemberId = member.Id,
        FirstName = member.Firstname,
        LastName = member.Lastname,
    };

    protected Expression<Func<LeagueMemberEntity, MemberModel>> MapToMemberModelExpression(bool includeProfile = false) => leagueMember => new()
    {
        MemberId = leagueMember.Member.Id,
        Firstname = leagueMember.Member.Firstname,
        Lastname = leagueMember.Member.Lastname,
        IRacingId = leagueMember.Member.IRacingId,
        TeamId = leagueMember.TeamId,
        TeamName = leagueMember.Team == null ? string.Empty : leagueMember.Team.Name,
        Number = leagueMember.Number,
        DiscordId = includeProfile ? leagueMember.DiscordId : string.Empty,
        CountryFlag = leagueMember.CountryFlag,
        Profile = includeProfile ? leagueMember.Profile : new(),
    };
}
