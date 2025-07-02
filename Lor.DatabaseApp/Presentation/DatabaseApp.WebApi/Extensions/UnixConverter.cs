namespace DatabaseApp.WebApi.Extensions;

public static class UnixConverter
{
    public static long ToUnixTime(this DateOnly dateOnly) =>
        new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)).ToUnixTimeSeconds();
}