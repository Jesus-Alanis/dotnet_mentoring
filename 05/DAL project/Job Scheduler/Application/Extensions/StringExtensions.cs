using Cronos;

namespace Application.Extensions;

public static class StringExtensions
{
    public static DateTimeOffset? GetNextExecution(
        this string cronExpression,
        DateTimeOffset? afterTime = null,
        string timeZoneId = "UTC")
    {
        if (!CronExpression.TryParse(cronExpression, CronFormat.Standard, out var cron))
            return null;

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var baseTime = afterTime ?? DateTimeOffset.UtcNow;

        return cron.GetNextOccurrence(baseTime, timeZone);
    }
}