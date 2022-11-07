namespace iRLeagueApiCore.Services.ResultService.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ForEeach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach(var item in enumerable)
            {
                action(item);
            }
            return enumerable;
        }
    }
}
