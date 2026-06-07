using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Services.Data.DriverPoints;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.DriverPoints;

public class DriverPointsService : IDriverPointsService
{
    private readonly IDriverPointsRepository repo;

    public DriverPointsService(IDriverPointsRepository repo)
    {
        this.repo = repo;
    }

    public async Task<DriverPointsAccountModel?> GetAccountAsync(long leagueId, long accountId)
    {
        var account = await repo.GetAccountWithEntriesAsync(leagueId, accountId);
        if (account == null) return null;

        await EvaluateExpiryRulesAsync(account);

        var activeEntries = account.Entries.Where(e => e.ArchivedOn == null).Select(e => new DriverPointsEntryModel
        {
            AccountEntryId = e.AccountEntryId,
            AccountId = e.AccountId,
            CreatedOn = e.CreatedOn,
            CreatedById = e.CreatedById,
            Description = e.Description,
            Value = e.Value,
            EntryKindId = e.EntryKindId,
            ArchivedOn = e.ArchivedOn,
            SourceRef = e.SourceRef
        }).ToList();

        return new DriverPointsAccountModel
        {
            AccountId = account.AccountId,
            MemberId = account.MemberId,
            Name = account.Name,
            Description = account.Description,
            CreatedOn = account.CreatedOn,
            ActiveEntries = activeEntries,
            CurrentPoints = activeEntries.Sum(x => x.Value != null ? x.Value.Points : 0)
        };
    }

    public async Task<IEnumerable<DriverPointsAccountModel>> GetAccountsForMemberAsync(long leagueId, long memberId)
    {
        var accounts = (await repo.GetAccountsForMemberAsync(leagueId, memberId)).ToList();

        foreach (var account in accounts)
        {
            await EvaluateExpiryRulesAsync(account);
        }

        return accounts.Select(account => new DriverPointsAccountModel
        {
            AccountId = account.AccountId,
            MemberId = account.MemberId,
            Name = account.Name,
            Description = account.Description,
            CreatedOn = account.CreatedOn,
            ActiveEntries = account.Entries.Where(e => e.ArchivedOn == null).Select(e => new DriverPointsEntryModel
            {
                AccountEntryId = e.AccountEntryId,
                AccountId = e.AccountId,
                CreatedOn = e.CreatedOn,
                CreatedById = e.CreatedById,
                Description = e.Description,
                Value = e.Value,
                EntryKindId = e.EntryKindId,
                ArchivedOn = e.ArchivedOn,
                SourceRef = e.SourceRef
            }),
            CurrentPoints = account.Entries.Where(e => e.ArchivedOn == null).Sum(e => e.Value != null ? e.Value.Points : 0)
        });
    }

    public async Task<DriverPointsAccountModel?> CreateAccountAsync(long leagueId, CreateDriverPointsAccountModel model, long createdById)
    {
        var account = new DriverPointsAccountEntity
        {
            LeagueId = leagueId,
            MemberId = model.MemberId,
            Name = model.Name,
            Description = model.Description,
            CreatedOn = DateTime.UtcNow
        };
        await repo.AddAccountWithSeasonsAsync(account, model.SeasonIds ?? Enumerable.Empty<long>());
        return await GetAccountAsync(account.LeagueId, account.AccountId);
    }

    public async Task<DriverPointsEntryModel> AddEntryAsync(long leagueId, long accountId, CreateDriverPointsEntryModel model)
    {
        var entry = new DriverPointsEntryEntity
        {
            LeagueId = leagueId,
            AccountId = accountId,
            CreatedOn = DateTime.UtcNow,
            CreatedById = model.CreatedById,
            Description = model.Description,
            Value = model.Value,
            SourceRef = model.SourceRef,
            EntryKindId = model.EntryKindId
        };
        await repo.AddEntryAsync(entry);

        return new DriverPointsEntryModel
        {
            AccountEntryId = entry.AccountEntryId,
            AccountId = entry.AccountId,
            CreatedOn = entry.CreatedOn,
            CreatedById = entry.CreatedById,
            Description = entry.Description,
            Value = entry.Value,
            EntryKindId = entry.EntryKindId,
            ArchivedOn = entry.ArchivedOn,
            SourceRef = entry.SourceRef
        };
    }

    public async Task ArchiveEntryAsync(long leagueId, long entryId, long userId)
    {
        await repo.ArchiveEntriesAsync(leagueId, new[] { entryId }, DateTime.UtcNow);
    }

    // EntryKind operations
    public async Task<IEnumerable<DriverPointsEntryKindModel>> GetEntryKindsAsync(long leagueId)
    {
        var kinds = (await repo.GetEntryKindsAsync(leagueId)).ToList();

        return kinds.Select(k => new DriverPointsEntryKindModel
        {
            EntryKindId = k.EntryKindId,
            LeagueId = k.LeagueId == 0 ? null : k.LeagueId,
            Name = k.Name,
            Description = k.Description,
            EntryType = k.EntryType
        });
    }

    public async Task<DriverPointsEntryKindModel?> GetEntryKindAsync(long entryKindId)
    {
        var k = await repo.GetEntryKindAsync(entryKindId);
        if (k == null) return null;
        return new DriverPointsEntryKindModel
        {
            EntryKindId = k.EntryKindId,
            LeagueId = k.LeagueId == 0 ? null : k.LeagueId,
            Name = k.Name,
            Description = k.Description,
            EntryType = k.EntryType
        };
    }

    public async Task<DriverPointsEntryKindModel?> CreateEntryKindAsync(long leagueId, CreateDriverPointsEntryKindModel model)
    {
        var entity = new DriverPointsEntryKindEntity
        {
            LeagueId = leagueId,
            Name = model.Name,
            Description = model.Description,
            EntryType = model.EntryType
        };
        var created = await repo.CreateEntryKindAsync(entity);
        return await GetEntryKindAsync(created.EntryKindId);
    }

    public async Task UpdateEntryKindAsync(long entryKindId, UpdateDriverPointsEntryKindModel model)
    {
        var entity = await repo.GetEntryKindAsync(entryKindId);
        if (entity == null) return;
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.EntryType = model.EntryType;
        await repo.UpdateEntryKindAsync(new DriverPointsEntryKindEntity { EntryKindId = entity.EntryKindId, LeagueId = entity.LeagueId, Name = entity.Name, Description = entity.Description, EntryType = entity.EntryType });
    }

    public async Task DeleteEntryKindAsync(long entryKindId)
    {
        await repo.DeleteEntryKindAsync(entryKindId);
    }

    private async Task EvvaluateEntryExpiryAsync(DriverPointsAccountEntity account, DriverPointsEntryEntity entry, List<DriverPointsExpiryRuleEntity> rules)
    {
        var now = DateTime.UtcNow;

        // find rule by entryKind first, then by penalty type
        DriverPointsExpiryRuleEntity? rule = null;
        if (entry.EntryKindId.HasValue)
        {
            rule = rules.FirstOrDefault(r => r.EntryKindId == entry.EntryKindId);
        }
        if (rule is null && entry.Value != null)
        {
            rule = rules.FirstOrDefault(r => r.PenaltyType == entry.Value.Type);
        }
        if (rule is null)
        {
            rule = rules.FirstOrDefault(r => r.PenaltyType == null && r.EntryKindId == null);
        }

        if (rule is null)
        {
            return;
        }

        // interval
        if (rule.IntervalDays.HasValue && entry.CreatedOn.AddDays(rule.IntervalDays.Value) <= now)
        {
            entry.ArchivedOn = now;
            return;
        }

        // events based
        if (rule.EventsDriven.HasValue || rule.EventsMissed.HasValue)
        {
            var seasonIds = account.Seasons.Select(s => s.SeasonId).ToArray();
            var memberId = account.MemberId;
            var counts = await repo.GetEventCountsAsync(account.LeagueId, memberId, seasonIds, entry.CreatedOn);
            var driven = counts.driven;
            var total = counts.total;
            var notDriven = total - driven;

            if (rule.EventsDriven.HasValue && driven >= rule.EventsDriven.Value)
            {
                entry.ArchivedOn = now;
                return;
            }
            if (rule.EventsMissed.HasValue && notDriven >= rule.EventsMissed.Value)
            {
                entry.ArchivedOn = now;
                return;
            }
        }

        // reset on new season
        if (rule.ResetOnNewSeason)
        {
            // TODO: determine season transition; placeholder logic
            var seasons = account.Seasons.OrderBy(s => s.SeasonId).ToList();
            if (seasons.Count > 1)
            {
                var createdSeason = seasons.FirstOrDefault(s => true); // placeholder
                var currentSeason = seasons.LastOrDefault();
                if (createdSeason != null && currentSeason != null && createdSeason.SeasonId != currentSeason.SeasonId)
                {
                    entry.ArchivedOn = now;
                    ;
                }
            }
        }
    }

    // Expiry evaluation
    public async Task EvaluateExpiryRulesAsync(DriverPointsAccountEntity account)
    {
        
        var rules = (await repo.GetExpiryRulesAsync(account.LeagueId)).ToList();
        var activeEntries = account.Entries.Where(e => e.ArchivedOn == null).ToList();
        var toArchive = new List<long>();
        foreach (var entry in activeEntries)
        {
            await EvvaluateEntryExpiryAsync(account, entry, rules);
            if (entry.ArchivedOn.HasValue)
            {
                toArchive.Add(entry.AccountEntryId);
            }
        }

        if (toArchive.Any())
        {
            await repo.ArchiveEntriesAsync(account.LeagueId, toArchive, DateTime.UtcNow);
        }
    }
}
