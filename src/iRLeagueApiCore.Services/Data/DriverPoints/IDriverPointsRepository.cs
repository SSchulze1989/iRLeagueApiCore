using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.Data.DriverPoints;

public interface IDriverPointsRepository
{
    Task<DriverPointsAccountEntity> GetAccountWithEntriesAsync(long leagueId, long accountId);
    Task<IEnumerable<DriverPointsAccountEntity>> GetAccountsForMemberAsync(long leagueId, long memberId);
    Task<long> AddAccountAsync(DriverPointsAccountEntity account);
    Task<long> AddEntryAsync(DriverPointsEntryEntity entry);
    Task ArchiveEntryAsync(long leagueId, long entryId, DateTime archivedOn);
    Task<IEnumerable<DriverPointsExpiryRuleEntity>> GetExpiryRulesAsync(long leagueId);
    Task<(int driven, int total)> GetEventCountsAsync(long leagueId, long memberId, IEnumerable<long> seasonIds, DateTime since);

    Task<IEnumerable<DriverPointsEntryKindEntity>> GetEntryKindsAsync(long leagueId);
    Task<DriverPointsEntryKindEntity> GetEntryKindAsync(long entryKindId);
    Task<DriverPointsEntryKindEntity> CreateEntryKindAsync(DriverPointsEntryKindEntity entity);
    Task UpdateEntryKindAsync(DriverPointsEntryKindEntity entity);
    Task DeleteEntryKindAsync(long entryKindId);

    // batch operations
    Task<long> AddAccountWithSeasonsAsync(DriverPointsAccountEntity account, IEnumerable<long> seasonIds);
    Task ArchiveEntriesAsync(long leagueId, IEnumerable<long> entryIds, DateTime archivedOn);
}
