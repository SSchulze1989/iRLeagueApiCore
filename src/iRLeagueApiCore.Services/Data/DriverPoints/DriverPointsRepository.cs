using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Data.DriverPoints;

public class DriverPointsRepository : IDriverPointsRepository
{
    private readonly LeagueDbContext db;

    public DriverPointsRepository(LeagueDbContext db)
    {
        this.db = db;
    }

    public async Task<DriverPointsAccountEntity> GetAccountWithEntriesAsync(long leagueId, long accountId)
    {
        return await db.Set<DriverPointsAccountEntity>()
            .Include(a => a.Entries)
                .ThenInclude(e => e.EntryKind)
            .Include(a => a.Seasons)
            .FirstOrDefaultAsync(a => a.LeagueId == leagueId && a.AccountId == accountId);
    }

    public async Task<IEnumerable<DriverPointsAccountEntity>> GetAccountsForMemberAsync(long leagueId, long memberId)
    {
        return await db.Set<DriverPointsAccountEntity>()
            .Include(a => a.Entries)
                .ThenInclude(e => e.EntryKind)
            .Include(a => a.Seasons)
            .Where(a => a.LeagueId == leagueId && a.MemberId == memberId)
            .ToListAsync();
    }

    public async Task<long> AddAccountAsync(DriverPointsAccountEntity account)
    {
        db.Add(account);
        await db.SaveChangesAsync();
        return account.AccountId;
    }

    public async Task<long> AddEntryAsync(DriverPointsEntryEntity entry)
    {
        db.Add(entry);
        await db.SaveChangesAsync();
        return entry.AccountEntryId;
    }

    public async Task ArchiveEntryAsync(long leagueId, long entryId, DateTime archivedOn)
    {
        var entry = await db.Set<DriverPointsEntryEntity>().FirstOrDefaultAsync(e => e.LeagueId == leagueId && e.AccountEntryId == entryId);
        if (entry == null) return;
        entry.ArchivedOn = archivedOn;
        await db.SaveChangesAsync();
    }

    public async Task<long> AddAccountWithSeasonsAsync(DriverPointsAccountEntity account, IEnumerable<long> seasonIds)
    {
        db.Add(account);
        await db.SaveChangesAsync();
        if (seasonIds != null)
        {
            foreach (var seasonId in seasonIds)
            {
                db.Add(new DriverPointsAccountSeason { LeagueId = account.LeagueId, AccountId = account.AccountId, SeasonId = seasonId });
            }
            await db.SaveChangesAsync();
        }
        return account.AccountId;
    }

    public async Task ArchiveEntriesAsync(long leagueId, IEnumerable<long> entryIds, DateTime archivedOn)
    {
        var entries = await db.Set<DriverPointsEntryEntity>().Where(e => e.LeagueId == leagueId && entryIds.Contains(e.AccountEntryId)).ToListAsync();
        foreach (var entry in entries)
        {
            entry.ArchivedOn = archivedOn;
        }
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<DriverPointsExpiryRuleEntity>> GetExpiryRulesAsync(long leagueId)
    {
        return await db.Set<DriverPointsExpiryRuleEntity>().Where(r => r.LeagueId == leagueId).ToListAsync();
    }

        public async Task<(int driven, int total)> GetEventCountsAsync(long leagueId, long memberId, IEnumerable<long> seasonIds, DateTime since)
    {
        var seasons = seasonIds.ToArray();
        if (!seasons.Any()) return (0, 0);

        // Count driven events: distinct ScoredEventResult.ResultId where member had a ScoredResultRow in that event since date
        var driven = await db.Set<ScoredResultRowEntity>()
            .Where(r => r.LeagueId == leagueId
                        && r.MemberId == memberId
                        && r.ScoredSessionResult != null
                        && r.ScoredSessionResult.ScoredEventResult != null
                        && r.ScoredSessionResult.ScoredEventResult.Event != null
                        && r.ScoredSessionResult.CreatedOn.HasValue
                        && seasons.Contains(r.ScoredSessionResult.ScoredEventResult.Event.Schedule.SeasonId)
                        && r.ScoredSessionResult.CreatedOn.Value >= since)
            .Select(r => r.ScoredSessionResult.ScoredEventResult.ResultId)
            .Distinct()
            .CountAsync();

        // Count total events in the seasons since date
        var total = await db.Set<ScoredEventResultEntity>()
            .Where(e => e.LeagueId == leagueId
                        && e.Event != null
                        && e.Event.Schedule != null
                        && seasons.Contains(e.Event.Schedule.SeasonId)
                        && e.CreatedOn.HasValue
                        && e.CreatedOn.Value >= since)
            .CountAsync();

        return (driven, total);
    }

    public async Task<IEnumerable<DriverPointsEntryKindEntity>> GetEntryKindsAsync(long leagueId)
    {
        return await db.Set<DriverPointsEntryKindEntity>().Where(k => k.LeagueId == leagueId || k.LeagueId == 0).ToListAsync();
    }

    public async Task<DriverPointsEntryKindEntity> GetEntryKindAsync(long entryKindId)
    {
        return await db.Set<DriverPointsEntryKindEntity>().FindAsync(entryKindId);
    }

    public async Task<DriverPointsEntryKindEntity> CreateEntryKindAsync(DriverPointsEntryKindEntity entity)
    {
        db.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateEntryKindAsync(DriverPointsEntryKindEntity entity)
    {
        db.Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteEntryKindAsync(long entryKindId)
    {
        var entity = await db.Set<DriverPointsEntryKindEntity>().FindAsync(entryKindId);
        if (entity == null) return;
        db.Remove(entity);
        await db.SaveChangesAsync();
    }
}
