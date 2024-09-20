namespace iRLeagueApiCore.Common.Models;

[DataContract]
public class BonusPointModel
{
    [DataMember]
    public BonusPointType Type { get; set; }
    [DataMember]
    public double Value { get; set; }
    [DataMember]
    public double Points { get; set; }
    [DataMember]
    public ICollection<FilterConditionModel> Conditions { get; set; } = new List<FilterConditionModel>();
    /// <summary>
    /// Maximum number of drivers/teams that are allowed to receive this bonus - if the conditions apply to more than this number the bonus will not be applied
    /// </summary>
    [DataMember]
    public int MaxCount { get; set; }
}