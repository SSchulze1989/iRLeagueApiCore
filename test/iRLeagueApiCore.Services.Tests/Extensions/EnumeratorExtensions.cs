using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.Tests.Extensions
{
    internal static class EnumeratorExtensions
    {
        /// <summary>
        /// Returns <see cref="IEnumerator{T}.Current" and calls <see cref="IEnumerator{T}.MoveNext()"/>/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator"></param>
        /// <returns>Current value</returns>
        public static T? Next<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            return default(T);
        }
    }
}
