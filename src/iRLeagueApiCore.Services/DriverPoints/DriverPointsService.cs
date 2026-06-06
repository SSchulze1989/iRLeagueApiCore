using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.DriverPoints;

public class DriverPointsService : IDriverPointsService
{
    private readonly LeagueDbContext db;

    public DriverPointsService(LeagueDbContext db)
    {
        this.db = db;
    }

    public async Task<DriverPointsAccountModel> GetAccountAsync(long leagueId, long accountId)
    {
        var account = await db.Set<DriverPointsAccountEntity>()
            .Include(a => a.Entries)
            .Include(a => a.Seasons)
            .FirstOrDefaultAsync(a => a.LeagueId == leagueId && a.AccountId == accountId);
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
        var accounts = await db.Set<DriverPointsAccountEntity>()
            .Include(a => a.Entries)
            .Include(a => a.Seasons)
            .Where(a => a.LeagueId == leagueId && a.MemberId == memberId)
            .ToListAsync();

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

    public async Task<DriverPointsAccountModel> CreateAccountAsync(CreateDriverPointsAccountModel model, long createdById)
    {
        var account = new DriverPointsAccountEntity
        {
            LeagueId = createdById, // <-- TODO: leagueId should be passed explicitly; using createdById as placeholder for now
            MemberId = model.MemberId,
            Name = model.Name,
            Description = model.Description,
            CreatedOn = DateTime.UtcNow
        };
        db.Add(account);
        await db.SaveChangesAsync();

        if (model.SeasonIds != null)
        {
            foreach (var seasonId in model.SeasonIds)
            {
                db.Add(new DriverPointsAccountSeason { LeagueId = account.LeagueId, AccountId = account.AccountId, SeasonId = seasonId });
            }
            await db.SaveChangesAsync();
        }

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
            EntryKind = await db.Set<DriverPointsEntryKindEntity>().FindAsync(model.EntryKindId)
        };
        db.Add(entry);
        await db.SaveChangesAsync();

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
        var entry = await db.Set<DriverPointsEntryEntity>().FirstOrDefaultAsync(e => e.LeagueId == leagueId && e.AccountEntryId == entryId);
        if (entry == null) return;
        entry.ArchivedOn = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    // EntryKind operations
    public async Task<IEnumerable<DriverPointsEntryKindModel>> GetEntryKindsAsync(long leagueId)
    {
        var kinds = await db.Set<DriverPointsEntryKindEntity>()
            .Where(k => k.LeagueId == leagueId || k.LeagueId == 0)
            .ToListAsync();

        return kinds.Select(k => new DriverPointsEntryKindModel
        {
            EntryKindId = k.EntryKindId,
            LeagueId = k.LeagueId == 0 ? null : k.LeagueId,
            Name = k.Name,
            Description = k.Description,
            EntryType = k.EntryType
        });
    }

    public async Task<DriverPointsEntryKindModel> GetEntryKindAsync(long entryKindId)
    {
        var k = await db.Set<DriverPointsEntryKindEntity>().FindAsync(entryKindId);
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

    public async Task<DriverPointsEntryKindModel> CreateEntryKindAsync(long leagueId, CreateDriverPointsEntryKindModel model)
    {
        var entity = new DriverPointsEntryKindEntity
        {
            LeagueId = leagueId,
            Name = model.Name,
            Description = model.Description,
            EntryType = model.EntryType
        };
        db.Add(entity);
        await db.SaveChangesAsync();
        return await GetEntryKindAsync(entity.EntryKindId);
    }

    public async Task UpdateEntryKindAsync(long entryKindId, UpdateDriverPointsEntryKindModel model)
    {
        var entity = await db.Set<DriverPointsEntryKindEntity>().FindAsync(entryKindId);
        if (entity == null) return;
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.EntryType = model.EntryType;
        await db.SaveChangesAsync();
    }

    public async Task DeleteEntryKindAsync(long entryKindId)
    {
        var entity = await db.Set<DriverPointsEntryKindEntity>().FindAsync(entryKindId);
        if (entity == null) return;
        db.Remove(entity);
        await db.SaveChangesAsync();
    }

    // Expiry evaluation
    public async Task EvaluateExpiryRulesAsync(DriverPointsAccountEntity account)
    {
        var now = DateTime.UtcNow;
        var rules = await db.Set<DriverPointsExpiryRuleEntity>().Where(r => r.LeagueId == account.LeagueId).ToListAsync();
        var activeEntries = account.Entries.Where(e => e.ArchivedOn == null).ToList();
        foreach (var entry in activeEntries)
        {
            // find rule by entryKind first, then by penalty type
            DriverPointsExpiryRuleEntity rule = null;
            if (entry.EntryKindId.HasValue)
            {
                rule = rules.FirstOrDefault(r => r.EntryKindId == entry.EntryKindId);
            }
            if (rule == null && entry.Value != null)
            {
                rule = rules.FirstOrDefault(r => r.PenaltyType == entry.Value.Type);
            }
            if (rule == null)
            {
                rule = rules.FirstOrDefault(r => r.PenaltyType == null && r.EntryKindId == null);
            }

            if (rule == null) continue;

            // interval
            if (rule.IntervalDays.HasValue && entry.CreatedOn.AddDays(rule.IntervalDays.Value) <= now)
            {
                entry.ArchivedOn = now;
                continue;
            }

            // events based
            if (rule.EventsDriven.HasValue || rule.EventsMissed.HasValue)
            {
                var seasonIds = account.Seasons.Select(s => s.SeasonId).ToArray();
                var counts = await db.Set<ScoredResultRowEntity>()
                    .Where(r => r.LeagueId == account.LeagueId && r.MemberId == account.MemberId && seasonIds.Contains(r.SimSessionType))
                    .CountAsync();
                // TODO: implement accurate event driven counting; placeholder
                var driven = counts;
                var total = counts;
                var notDriven = total - driven;

                if (rule.EventsDriven.HasValue && driven >= rule.EventsDriven.Value)
                {
                    entry.ArchivedOn = now;
                    continue;
                }
                if (rule.EventsMissed.HasValue && notDriven >= rule.EventsMissed.Value)
                {
                    entry.ArchivedOn = now;
                    continue;
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
                        continue;
                    }
                }
            }
        }

        await db.SaveChangesAsync();
    }
}
