namespace DayCounter.Models;

public static class DayCount
{
    public static int Since(DateTime start, DateTime today) => Math.Max(0, (today.Date - start.Date).Days);
}
