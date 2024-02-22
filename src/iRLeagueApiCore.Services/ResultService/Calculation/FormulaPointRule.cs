
using iRLeagueApiCore.Services.ResultService.Models;
using MySqlX.XDevAPI.Relational;
using NCalc;

namespace iRLeagueApiCore.Services.ResultService.Calculation;
internal class FormulaPointRule : CalculationPointRuleBase
{
    private string _formula;
    private SessionCalculationResult _sessionData;
    private bool _allowNegativePoints;
    private static IDictionary<string, FormulaParameter> _parameters = FormulaParameters.ParameterDict;

    public FormulaPointRule(string formula, SessionCalculationResult sessionData, bool allowNegativePoints)
    {
        _formula = formula;
        _sessionData = sessionData;
        _allowNegativePoints = allowNegativePoints;
    }

    public override IReadOnlyList<T> ApplyPoints<T>(IReadOnlyList<T> rows)
    {
        // prepare parameters
        var e = new NCalc.Expression(_formula, EvaluateOptions.IterateParameters);
        foreach (var parameter in _parameters)
        {
            e.Parameters[parameter.Key] = rows.Select(row => parameter.Value.valueFunc.Invoke(_sessionData, row)).ToArray();
        }
        // calculate
        if (e.Evaluate() is not IList<object> points)
        {
            return rows;
        }
        // assign points to rows
        foreach (var (row, rowPoints) in rows.Zip(points))
        {
            row.RacePoints = Convert.ToDouble(rowPoints);
            if (!_allowNegativePoints)
            {
                row.RacePoints = Math.Max(row.RacePoints, 0);
            }
        }
        return rows;
    }
}
