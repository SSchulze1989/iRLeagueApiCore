﻿using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Calculation.Filters;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal abstract class CalculationPointRuleBase : PointRule<ResultRowCalculationResult>
{
    public FilterGroupRowFilter<ResultRowCalculationResult> PointFilters { get; set; } = new();
    public FilterGroupRowFilter<ResultRowCalculationResult> ChampSeasonFilters { get; set; } = new();
    public FilterGroupRowFilter<ResultRowCalculationResult> ResultFilters { get; set; } = new();
    public IEnumerable<SortOptions> PointSortOptions { get; set; } = [];
    public IEnumerable<SortOptions> FinalSortOptions { get; set; } = [];
    public IEnumerable<BonusPointConfiguration> BonusPoints { get; set; } = [];
    public IEnumerable<AutoPenaltyConfigurationData> AutoPenalties { get; set; } = [];

    public override FilterGroupRowFilter<ResultRowCalculationResult> GetResultFilters() => ResultFilters;
    public override FilterGroupRowFilter<ResultRowCalculationResult> GetPointFilters() => PointFilters;
    public override FilterGroupRowFilter<ResultRowCalculationResult> GetChampSeasonFilters() => ChampSeasonFilters;
    public override IEnumerable<AutoPenaltyConfigurationData> GetAutoPenalties() => AutoPenalties;
    public override IEnumerable<BonusPointConfiguration> GetBonusPoints() => BonusPoints;

    public override IReadOnlyList<T> SortFinal<T>(IEnumerable<T> rows)
    {
        foreach (var sortOption in FinalSortOptions.Reverse())
        {
            rows = rows.OrderBy(sortOption.GetSortingValue<T>());
        }
        return rows.ToList();
    }

    public override IReadOnlyList<T> SortForPoints<T>(IEnumerable<T> rows)
    {
        foreach (var sortOptions in PointSortOptions.Reverse())
        {
            rows = rows.OrderBy(sortOptions.GetSortingValue<T>());
        }
        return rows.ToList();
    }
}
