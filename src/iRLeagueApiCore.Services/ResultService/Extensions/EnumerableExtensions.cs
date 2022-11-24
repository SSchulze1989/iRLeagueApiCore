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

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable, Random? random = default)
        {
            random ??= new();
            return enumerable
                .Select(item => new { item, rnd = random.Next() })
                .OrderBy(x => x.rnd)
                .Select(x => x.item);
                
        }
    }
}
