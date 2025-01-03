namespace iRLeagueApiCore.Common.Models;

[DataContract]
public class PutDropweekOverrideModel
{
    [DataMember]
    public bool ShouldDrop { get; set; }
}
