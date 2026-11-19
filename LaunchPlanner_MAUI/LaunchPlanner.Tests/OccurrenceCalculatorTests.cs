using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Scheduling;

namespace LaunchPlanner.Tests;

public sealed class OccurrenceCalculatorTests
{
    private static ScheduledEvent Event(Periodicity period, DateTime start, TimeSpan? duration = null) => new()
    {
        Start = start,
        Finish = new DateTime(2099, 1, 1),
        Duration = duration ?? TimeSpan.FromMinutes(1),
        Period = period,
        Enabled = true,
    };

    [Fact]
    public void Latest_IsNull_BeforeTheStart()
    {
        var ev = Event(Periodicity.Day, new DateTime(2026, 3, 10, 8, 0, 0));

        Assert.Null(OccurrenceCalculator.Latest(ev, new DateTime(2026, 3, 10, 7, 59, 59)));
    }

    [Theory]
    [InlineData(Periodicity.SingleShot)]
    [InlineData(Periodicity.NonPeriodical)]
    public void SingleShot_OccursOnlyAtTheStart(Periodicity period)
    {
        var start = new DateTime(2026, 3, 10, 8, 0, 0);
        var ev = Event(period, start);

        Assert.Equal(start, OccurrenceCalculator.Latest(ev, start.AddYears(1)));
        Assert.Equal(start, OccurrenceCalculator.Next(ev, start.AddSeconds(-1)));
        Assert.Null(OccurrenceCalculator.Next(ev, start));
    }

    [Theory]
    [InlineData(Periodicity.Second, "2026-03-10 09:37:20", "2026-03-10 09:37:20")]
    [InlineData(Periodicity.OneMinute, "2026-03-10 09:37:20", "2026-03-10 09:37:05")]
    [InlineData(Periodicity.QuarterOfHour, "2026-03-10 09:37:20", "2026-03-10 09:25:05")]
    [InlineData(Periodicity.HalfHour, "2026-03-10 09:37:20", "2026-03-10 09:10:05")]
    [InlineData(Periodicity.Hour, "2026-03-10 09:37:20", "2026-03-10 09:10:05")]
    [InlineData(Periodicity.Day, "2026-03-12 07:00:00", "2026-03-11 08:10:05")]
    [InlineData(Periodicity.Weekly, "2026-03-20 07:00:00", "2026-03-17 08:10:05")]
    public void Latest_StaysAlignedToTheStart(Periodicity period, string now, string expected)
    {
        var ev = Event(period, new DateTime(2026, 3, 10, 8, 10, 5));

        Assert.Equal(DateTime.Parse(expected), OccurrenceCalculator.Latest(ev, DateTime.Parse(now)));
    }

    [Fact]
    public void Day_KeepsTheWallClockTimeAcrossDaylightSaving()
    {
        // In Italia l'ora legale del 2026 inizia il 29 marzo
        var ev = Event(Periodicity.Day, new DateTime(2026, 3, 28, 7, 45, 0));

        Assert.Equal(new DateTime(2026, 3, 30, 7, 45, 0),
            OccurrenceCalculator.Next(ev, new DateTime(2026, 3, 29, 12, 0, 0)));
    }

    [Theory]
    [InlineData(Periodicity.Monday, DayOfWeek.Monday)]
    [InlineData(Periodicity.Tuesday, DayOfWeek.Tuesday)]
    [InlineData(Periodicity.Wednesday, DayOfWeek.Wednesday)]
    [InlineData(Periodicity.Thursday, DayOfWeek.Thursday)]
    [InlineData(Periodicity.Friday, DayOfWeek.Friday)]
    [InlineData(Periodicity.Saturday, DayOfWeek.Saturday)]
    [InlineData(Periodicity.Sunday, DayOfWeek.Sunday)]
    public void NamedDay_OccursOnlyOnThatDay(Periodicity period, DayOfWeek expected)
    {
        var ev = Event(period, new DateTime(2026, 3, 1, 21, 15, 0));

        var next = OccurrenceCalculator.Next(ev, new DateTime(2026, 3, 11, 22, 0, 0));
        var latest = OccurrenceCalculator.Latest(ev, new DateTime(2026, 3, 11, 22, 0, 0));

        Assert.Equal(expected, next!.Value.DayOfWeek);
        Assert.Equal(expected, latest!.Value.DayOfWeek);
        Assert.Equal(TimeSpan.FromDays(7), next - latest);
    }

    [Fact]
    public void Weekday_SkipsTheWeekend()
    {
        var ev = Event(Periodicity.Weekday, new DateTime(2018, 10, 2, 14, 50, 0));
        var saturday = new DateTime(2026, 10, 31, 12, 0, 0);

        Assert.Equal(new DateTime(2026, 10, 30, 14, 50, 0), OccurrenceCalculator.Latest(ev, saturday));
        Assert.Equal(new DateTime(2026, 11, 2, 14, 50, 0), OccurrenceCalculator.Next(ev, saturday));
    }

    [Fact]
    public void Weekend_SkipsTheWorkingDays()
    {
        var ev = Event(Periodicity.Weekend, new DateTime(2026, 1, 1, 9, 0, 0));
        var sunday = new DateTime(2026, 11, 1, 10, 0, 0);

        Assert.Equal(new DateTime(2026, 11, 1, 9, 0, 0), OccurrenceCalculator.Latest(ev, sunday));
        Assert.Equal(new DateTime(2026, 11, 7, 9, 0, 0), OccurrenceCalculator.Next(ev, sunday));
    }

    [Fact]
    public void Latest_IsNull_WhenTheOnlyPastDayIsFilteredOut()
    {
        var sunday = new DateTime(2026, 3, 1, 8, 0, 0);
        var ev = Event(Periodicity.Weekday, sunday);

        Assert.Null(OccurrenceCalculator.Latest(ev, sunday.AddHours(1)));
    }

    [Fact]
    public void Month_ClampsToTheEndOfShortMonthsWithoutLosingTheDay()
    {
        var ev = Event(Periodicity.Month, new DateTime(2026, 1, 31, 10, 0, 0));

        Assert.Equal(new DateTime(2026, 2, 28, 10, 0, 0),
            OccurrenceCalculator.Next(ev, new DateTime(2026, 2, 1)));
        Assert.Equal(new DateTime(2026, 3, 31, 10, 0, 0),
            OccurrenceCalculator.Next(ev, new DateTime(2026, 3, 1)));
    }

    [Fact]
    public void Year_RepeatsOnTheSameDate()
    {
        var ev = Event(Periodicity.Year, new DateTime(2020, 5, 4, 10, 0, 0));

        Assert.Equal(new DateTime(2026, 5, 4, 10, 0, 0),
            OccurrenceCalculator.Latest(ev, new DateTime(2026, 10, 1)));
        Assert.Equal(new DateTime(2027, 5, 4, 10, 0, 0),
            OccurrenceCalculator.Next(ev, new DateTime(2026, 10, 1)));
    }

    [Theory]
    [InlineData("00:00:01", "2026-03-10 09:37:00", "2026-03-10 09:37:30")]
    [InlineData("00:01:00", "2026-03-10 09:37:00", "2026-03-10 10:20:30")]
    [InlineData("01:00:00", "2026-03-10 09:37:00", "2026-03-11 08:20:30")]
    [InlineData("1.00:00:00", "2026-03-10 09:37:00", "2026-04-10 08:20:30")]
    public void Custom_DerivesThePeriodFromTheDuration(string duration, string after, string expected)
    {
        var ev = Event(Periodicity.Custom, new DateTime(2026, 3, 10, 8, 20, 30), TimeSpan.Parse(duration));

        Assert.Equal(DateTime.Parse(expected), OccurrenceCalculator.Next(ev, DateTime.Parse(after)));
    }

    [Fact]
    public void Custom_LongerThanADay_NeverRepeats()
    {
        var start = new DateTime(2026, 3, 10, 8, 20, 30);
        var ev = Event(Periodicity.Custom, start, TimeSpan.FromDays(2));

        Assert.Null(OccurrenceCalculator.Next(ev, start));
    }

    [Fact]
    public void Next_IsNull_PastTheFinish()
    {
        var start = new DateTime(2026, 3, 10, 8, 0, 0);
        var ev = Event(Periodicity.Day, start) with { Finish = start.AddDays(1).AddHours(-1) };

        Assert.Null(OccurrenceCalculator.Next(ev, start));
    }
}
