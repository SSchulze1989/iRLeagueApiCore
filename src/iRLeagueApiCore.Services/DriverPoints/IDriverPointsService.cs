using System.Collections.Generic;
using System.Threading.Tasks;
using iRLeagueApiCore.Common.Models.DriverPoints;

namespace iRLeagueApiCore.Services.DriverPoints;

public interface IDriverPointsService
{
    Task<DriverPointsAccountModel> GetAccountAsync(long leagueId, long accountId);
    Task<IEnumerable<DriverPointsAccountModel>> GetAccountsForMemberAsync(long leagueId, long memberId);
    Task<DriverPointsAccountModel> CreateAccountAsync(CreateDriverPointsAccountModel model, long createdById);
    Task<DriverPointsEntryModel> AddEntryAsync(long leagueId, long accountId, CreateDriverPointsEntryModel model);
    Task ArchiveEntryAsync(long leagueId, long entryId, long userId);

    // EntryKind operations
    Task<IEnumerable<DriverPointsEntryKindModel>> GetEntryKindsAsync(long leagueId);
    Task<DriverPointsEntryKindModel> GetEntryKindAsync(long entryKindId);
    Task<DriverPointsEntryKindModel> CreateEntryKindAsync(long leagueId, CreateDriverPointsEntryKindModel model);
    Task UpdateEntryKindAsync(long entryKindId, UpdateDriverPointsEntryKindModel model);
    Task DeleteEntryKindAsync(long entryKindId);
}
