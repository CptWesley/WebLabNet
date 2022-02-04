using System;

namespace WebLabNet.Internal;

/// <summary>
/// Provides extension methods to the <see cref="DateTime"/> class.
/// </summary>
internal static class DateTimeExtensions
{
    /// <summary>
    /// Gets the number of seconds since first of January 1970.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The number of seconds since the first of January 1970.</returns>
    public static long ToEpochSeconds(this DateTime date)
        => (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;

    /// <summary>
    /// Gets the number of milliseconds since first of January 1970.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The number of milliseconds since the first of January 1970.</returns>
    public static long ToEpochMilliseconds(this DateTime date)
        => (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
}
