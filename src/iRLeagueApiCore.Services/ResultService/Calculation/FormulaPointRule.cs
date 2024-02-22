
using iRLeagueApiCore.Services.ResultService.Models;
using NCalc;

namespace iRLeagueApiCore.Services.ResultService.Calculation;
internal class FormulaPointRule : CalculationPointRuleBase
{
    private string _formula;
    private SessionCalculationResult _sessionData;
    private static IDictionary<string, FormulaParameter> _parameters = FormulaParameters.ParameterDict;

    public FormulaPointRule(string formula, SessionCalculationResult sessionData)
    {
        _formula = formula;
        _sessionData = sessionData;
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
        }
        return rows;
    }
}
