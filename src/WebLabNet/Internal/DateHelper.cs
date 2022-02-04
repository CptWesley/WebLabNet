using System;
using System.Text.RegularExpressions;

namespace WebLabNet.Internal;

/// <summary>
/// Provides helper methods for dealing with times.
/// </summary>
internal static class DateHelper
{
    private static readonly Regex Matcher = new Regex(@"(\d{4})(\d{2})(\d{2})T(\d{2})(\d{2})(\d{2})(\+\d{4})*");

    /// <summary>
    /// Parses timestamps.
    /// </summary>
    /// <param name="time">The time string.</param>
    /// <returns>The parsed time.</returns>
    public static DateTime Parse(string time)
    {
        if (string.IsNullOrEmpty(time))
        {
            return default;
        }

        if (DateTime.TryParse(time, out DateTime result))
        {
            return result;
        }

        Match m = Matcher.Match(time);
        if (m.Success)
        {
            string year = m.Groups[1].Value;
            string month = m.Groups[2].Value;
            string day = m.Groups[3].Value;
            string hour = m.Groups[4].Value;
            string minute = m.Groups[5].Value;
            string second = m.Groups[6].Value;
            string timezone = m.Groups[7].Value ?? "+0000";
            string fixedTime = $"{year}-{month}-{day}T{hour}:{minute}:{second}{timezone}";

            if (DateTime.TryParse(fixedTime, out result))
            {
                return result;
            }
        }

        return default;
    }
}
