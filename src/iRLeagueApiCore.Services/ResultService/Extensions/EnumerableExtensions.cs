﻿namespace iRLeagueApiCore.Services.ResultService.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ForEeach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
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

    /// <summary>
    /// returns subset of values that are not null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <returns></returns>
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : notnull
    {
        return enumerable.Where(x => x is not null).OfType<T>();
    }

    /// <summary>
    /// Returns true when the collection does not contain any elements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <returns></returns>
    public static bool None<T>(this IEnumerable<T> enumerable) => !enumerable.Any();
    /// <summary>
    /// Returns true when the collection does not contain any elements with the given predicate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static bool None<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) => !enumerable.Any(predicate);
}
