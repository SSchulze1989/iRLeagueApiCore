using Google.Protobuf.WellKnownTypes;
using iRLeagueApiCore.Common.Models;
using System.Text.Json.Serialization;

namespace iRLeagueDatabaseCore.Models;

public sealed class PenaltyValue
{
    public PenaltyType Type { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double Points { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TimeSpan Time { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Positions { get; set; }

    public static implicit operator PenaltyModel(PenaltyValue value) => new()
    {
        Type = value.Type,
        Points = (int)value.Points,
        Time = value.Time,
        Positions = value.Positions,
    };

    public static implicit operator PenaltyValue(PenaltyModel model) => new()
    {
        Type = model.Type,
        Points = model.Points,
        Time = model.Time,
        Positions = model.Positions,
    };
}
