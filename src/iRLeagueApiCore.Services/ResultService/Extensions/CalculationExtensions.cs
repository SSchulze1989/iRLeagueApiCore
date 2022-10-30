using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.ResultService.Extensions
{
    public static class CalculationExtensions
    {
        public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey? key)
        {
            if (key == null)
            {
                return default;
            }
            dictionary.TryGetValue(key, out TValue? value);
            return value;
        }

        public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Nullable<TKey> key) where TKey : struct
        {
            return GetOrDefault(dictionary, key);
        }

        public static IEnumerable<TValue> GetMultiple<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            return dictionary
                .Where(x => keys.Contains(x.Key))
                .Select(x => x.Value);
        }
    }
}
