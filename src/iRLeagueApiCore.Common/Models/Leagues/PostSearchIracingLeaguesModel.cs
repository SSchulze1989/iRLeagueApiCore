namespace iRLeagueApiCore.Common.Models.Leagues;

[DataContract]
public sealed class PostSearchIracingLeaguesModel
{
    [DataMember]
    public string Search { get; set; } = string.Empty;
}
