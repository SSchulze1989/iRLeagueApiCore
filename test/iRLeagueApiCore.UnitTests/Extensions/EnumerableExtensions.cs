using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.UnitTests.Extensions
{
    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> list, Random random)
        {
            return list.ElementAt(random.Next(list.Count()));
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list, Random random = default)
        {
            random ??= new();
            return list.OrderBy(x => random.Next());
        }
    }
}
