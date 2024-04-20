namespace iRLeagueApiCore.Common.Models.Leagues;

[DataContract]
public sealed class SearchIracingLeagueResultModel
{
    [DataMember]
    public int IracingLeagueId { get; set; }
    [DataMember]
    public int OwnerId { get; set; }
    [DataMember]
    public string OwnerDisplayName {  get; set; } = string.Empty;
    [DataMember]
    public string Name { get; set; } = string.Empty;
}
