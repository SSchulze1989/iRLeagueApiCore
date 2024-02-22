
using iRLeagueApiCore.Services.ResultService.Models;
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
        foreach(var row in rows)
        {
            var e = new NCalc.Expression(_formula);
            foreach(var parameter in _parameters)
            {
                e.Parameters[parameter.Key] = parameter.Value.valueFunc.Invoke(_sessionData, row);
            }
            var eval = e.Evaluate();
            row.RacePoints = Convert.ToDouble(eval);
            if (!_allowNegativePoints)
            {
                row.RacePoints = Math.Max(row.RacePoints, 0);
            }
        }
        return rows;
    }
}
