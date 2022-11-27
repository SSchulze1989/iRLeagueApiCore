using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Services.ResultService.Extensions;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal sealed class MemberSessionCalculationService : CalculationServiceBase
{
    private readonly SessionCalculationConfiguration config;

    public MemberSessionCalculationService(SessionCalculationConfiguration config)
    {
        this.config = config;
    }

    public override Task<SessionCalculationResult> Calculate(SessionCalculationData data)
    {
        if (data.LeagueId != config.LeagueId)
            throw new InvalidOperationException("LeagueId does not match");
        if (config.SessionId != null && data.SessionId != config.SessionId)
            throw new InvalidOperationException("SessionId does not match");

        var rows = data.ResultRows
            .Select(row => new ResultRowCalculationResult(row))
            .Where(row => row.MemberId != null)
            .ToList();
        var pointRule = config.PointRule;
        var finalRows = ApplyPointRule(rows, pointRule);

        var result = new SessionCalculationResult(data)
        {
            Name = config.Name,
            SessionResultId = config.SessionResultId,
            ResultRows = finalRows,
            SessionNr = data.SessionNr
        };
        (result.FastestAvgLapDriverMemberId, result.FastestAvgLap) = GetBestLapValue(finalRows, x => x.MemberId, x => x.AvgLapTime);
        (result.FastestLapDriverMemberId, result.FastestLap) = GetBestLapValue(finalRows, x => x.MemberId, x => x.FastestLapTime);
        (result.FastestQualyLapDriverMemberId, result.FastestQualyLap) = GetBestLapValue(finalRows, x => x.MemberId, x => x.QualifyingTime);
        result.CleanestDrivers = GetBestValues(rows, x => x.Incidents, x => x.MemberId, x => x.Min())
            .Select(x => x.id)
            .NotNull()
            .ToList();
        result.HardChargers = GetBestValues(rows.Where(HardChargerEligible), x => x.FinalPositionChange, x => x.MemberId, x => x.Max())
            .Select(x => x.id)
            .NotNull()
            .ToList();

        return Task.FromResult(result);
    }
}
