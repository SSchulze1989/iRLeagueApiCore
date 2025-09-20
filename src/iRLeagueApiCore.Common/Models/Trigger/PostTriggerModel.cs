namespace iRLeagueApiCore.Common.Models;
public class PostTriggerModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TriggerType TriggerType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
    public TriggerAction Action { get; set; }
    public Dictionary<string, object> ActionParameters { get; set; } = [];
}
