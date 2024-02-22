using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;
internal record FormulaParameter(string[] Aliases, string Description, Func<SessionCalculationResult, ResultRowCalculationData, object> valueFunc);

public static class FormulaParameters
{
    internal static IEnumerable<FormulaParameter> Parameters { get; } = new List<FormulaParameter>()
    {
        new(["pos", "position"], "Finish position", (_, rowData) => rowData.FinishPosition),
        new(["start", "start_position"], "Starting position", (_, rowData) => rowData.StartPosition),
        new(["irating"], "Irating at the start of the session", (_, rowData) => rowData.OldIrating),
        new(["sof", "strength_of_field"], "SOF - Strength of field (Irating)", (sessionData, _) => sessionData.Sof),
        new(["count", "driver_count"], "Number of drivers/teams in the result", (sessionData, _) => sessionData.ResultRows.Count()),
        new(["flap", "fastest_lap"], "Personal fastest lap", (_, rowData) => rowData.FastestLapTime.TotalSeconds),
        new(["qlap", "qualy_lap"], "Personal qualy lap", (_, rowData) => rowData.QualifyingTime.TotalSeconds),
        new(["avglap", "avg_lap"], "Personal avg. lap", (_, rowData) => rowData.AvgLapTime.TotalSeconds),
        new(["flapsession", "session_fastest_lap"], "Fastest lap in the session", (sessionData, _) => sessionData.FastestLap.TotalSeconds),
        new(["qlapsession", "session_qualy_lap"], "Fastest qaly lap in the session", (sessionData, _) => sessionData.FastestQualyLap.TotalSeconds),
        new(["avglapsession", "session_fastes_avg_lap"], "Fastest avg. lap in the session", (sessionData, _) => sessionData.FastestAvgLap.TotalSeconds),
    };

    public static IEnumerable<(string[] aliases, string description)> ParameterInfo => Parameters.Select(x => (x.Aliases, x.Description));
}
