namespace iRLeagueApiCore.Common.Models.Rosters;

[DataContract]
public class RosterInfoModel
{
    [DataMember]
    public long RosterId { get; set; }
    /// <summary>
    /// Name of the roster
    /// </summary>
    [DataMember]
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// [Optional] Description of the roster
    /// </summary>
    [DataMember]
    public string Description { get; set; } = string.Empty;
}
