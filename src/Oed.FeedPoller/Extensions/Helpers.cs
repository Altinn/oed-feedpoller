using System.Diagnostics.Contracts;

namespace Oed.FeedPoller.Extensions;
public static class Helpers
{
    public static bool ShouldRunUpdate(DateTime? timeToCheck = null)
    {
        var norwegianTime = timeToCheck ?? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "W. Europe Standard Time");

        // Always update between 0600 and 1759
        if (norwegianTime.Hour is >= 6 and < 18)
        {
            return true;
        }

        // Don't update at all between 0100 and 0459
        if (norwegianTime.Hour is >= 1 and < 5)
        {
            return false;
        }

        // At 0400-0559 and 1759-0100 update every half hour. This assumes this is ran at most every 5 minutes.
        if (norwegianTime.Minute is >= 58 or <= 2 or >= 28 and <= 32)
        {
            return true;
        }

        return false;

    }

    [Pure]
    public static IEnumerable<T> SkipNulls<T>(this IEnumerable<T?> enumerable) where T : class
    {
        return enumerable.Where(e => e != null).Select(e => e!);
    }

    [Pure]
    public static IEnumerable<T> SkipNulls<T>(this IEnumerable<T?> enumerable) where T : struct
    {
        return enumerable.Where(e => e.HasValue).Select(e => e!.Value);
    }

    public static Guid? StrToGuid(this string s)
    {
        Guid.TryParse(s, out Guid guid);
        return guid;
    }
}
