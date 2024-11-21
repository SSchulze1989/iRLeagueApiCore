namespace iRLeagueApiCore.Common.Models;

[DataContract]
public sealed class PostSearchIracingLeaguesModel
{
    [DataMember]
    public string Search { get; set; } = string.Empty;
}
