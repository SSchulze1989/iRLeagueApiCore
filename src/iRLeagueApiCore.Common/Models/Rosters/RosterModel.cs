namespace iRLeagueApiCore.Common.Models.Rosters;

[DataContract]
public sealed class RosterModel : PutRosterModel
{
    [DataMember]
    public long RosterId { get; set; }
    /// <summary>
    /// List of detailed information for each driver in the roster
    /// </summary>
    [DataMember]
    public IEnumerable<RosterEntryInfoModel> RosterEntries { get; set; } = [];
}
