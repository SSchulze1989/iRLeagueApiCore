﻿using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using MySqlX.XDevAPI.Relational;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal abstract class CalculationPointRuleBase : PointRule<ResultRowCalculationResult>
{
    public FilterGroupRowFilter<ResultRowCalculationResult> PointFilters { get; set; } = new(Array.Empty<(FilterCombination, RowFilter<ResultRowCalculationResult>)>());
    public FilterGroupRowFilter<ResultRowCalculationResult> ResultFilters { get; set; } = new(Array.Empty<(FilterCombination, RowFilter<ResultRowCalculationResult>)>());
    public IEnumerable<SortOptions> PointSortOptions { get; set; } = Array.Empty<SortOptions>();
    public IEnumerable<SortOptions> FinalSortOptions { get; set; } = Array.Empty<SortOptions>();
    public IDictionary<string, int> BonusPoints { get; set; } = new Dictionary<string, int>();
    public IEnumerable<AutoPenaltyConfigurationData> AutoPenalties { get; set; } = Array.Empty<AutoPenaltyConfigurationData>();

    public override FilterGroupRowFilter<ResultRowCalculationResult> GetResultFilters() => ResultFilters;
    public override FilterGroupRowFilter<ResultRowCalculationResult> GetPointFilters() => PointFilters;
    public override IEnumerable<AutoPenaltyConfigurationData> GetAutoPenalties() => AutoPenalties;
    public override IDictionary<string, int> GetBonusPoints() => BonusPoints;

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
