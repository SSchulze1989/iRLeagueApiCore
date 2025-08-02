namespace iRLeagueApiCore.Common.Models;

[DataContract]
public class BonusPointModel
{
    [DataMember]
    public string Name { get; set; } = string.Empty;
    [DataMember]
    public BonusPointType Type { get; set; }
    [DataMember]
    public double Value { get; set; }
    [DataMember]
    public double Points { get; set; }
    [DataMember]
    public ICollection<FilterConditionModel> Conditions { get; set; } = new List<FilterConditionModel>();
}