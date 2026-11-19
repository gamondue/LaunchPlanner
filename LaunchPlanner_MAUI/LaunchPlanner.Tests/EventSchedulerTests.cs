using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Scheduling;

namespace LaunchPlanner.Tests;

public sealed class EventSchedulerTests
{
    private static readonly DateTime Start = new(2026, 3, 9, 14, 50, 0); // lunedì

    private static ScheduledEvent Event(
        Periodicity period = Periodicity.Day, TimeSpan? duration = null, string description = "e") => new()
    {
        Start = Start,
        Finish = Start.AddYears(5),
        Duration = duration ?? TimeSpan.FromMinutes(15),
        Period = period,
        Description = description,
        Url = "https://example.org",
        Enabled = true,
    };

    private static List<ScheduledEvent> Due(ScheduledEvent ev, DateTime now, FiredLog? log = null)
        => EventScheduler.TakeDue([ev], now, log ?? new FiredLog()).Select(d => d.Event).ToList();

    [Fact]
    public void FiresInsideTheActivationWindow()
    {
        Assert.Single(Due(Event(), Start.AddDays(2).AddMinutes(10)));
    }

    [Fact]
    public void DoesNotFireAfterTheActivationWindow()
    {
        Assert.Empty(Due(Event(), Start.AddDays(2).AddMinutes(16)));
    }

    [Fact]
    public void FiresOncePerOccurrence()
    {
        var log = new FiredLog();

        Assert.Single(Due(Event(), Start, log));
        Assert.Empty(Due(Event(), Start.AddMinutes(1), log));
        Assert.Single(Due(Event(), Start.AddDays(1), log));
    }

    [Fact]
    public void ZeroDuration_StillFiresWhenTheWakeUpIsSlightlyLate()
    {
        var ev = Event(duration: TimeSpan.Zero);

        Assert.Single(Due(ev, Start.AddSeconds(30)));
        Assert.Empty(Due(ev, Start.AddMinutes(2)));
    }

    [Fact]
    public void DurationLongerThanThePeriod_WaitsForTheFirstOccurrenceAfterItsEnd()
    {
        var log = new FiredLog();
        var ev = Event(Periodicity.OneMinute, TimeSpan.FromMinutes(5));

        Assert.Single(Due(ev, Start, log));
        Assert.Empty(Due(ev, Start.AddMinutes(5), log));
        Assert.Single(Due(ev, Start.AddMinutes(6), log));
    }

    [Fact]
    public void IgnoresDisabledEvents()
    {
        Assert.Empty(Due(Event() with { Enabled = false }, Start));
    }

    [Fact]
    public void IgnoresEventsPastTheFinish()
    {
        Assert.Empty(Due(Event() with { Finish = Start.AddDays(1) }, Start.AddDays(1).AddSeconds(1)));
    }

    [Fact]
    public void AnEventWithoutOccurrences_DoesNotStopTheFollowingOnes()
    {
        var saturday = new DateTime(2026, 3, 14, 14, 50, 0);
        ScheduledEvent[] events = [Event(Periodicity.Weekday), Event(Periodicity.Saturday, description: "ok")];

        var due = EventScheduler.TakeDue(events, saturday, new FiredLog());

        Assert.Equal("ok", Assert.Single(due).Event.Description);
    }

    [Fact]
    public void NextWake_IsTheEarliestOccurrenceOfTheEnabledEvents()
    {
        ScheduledEvent[] events =
        [
            Event(Periodicity.Weekly),
            Event(Periodicity.Day) with { Enabled = false },
            Event(Periodicity.Hour),
        ];

        Assert.Equal(Start.AddHours(1), EventScheduler.NextWake(events, Start));
    }

    [Fact]
    public void NextWake_IsNull_WhenNothingIsLeft()
    {
        Assert.Null(EventScheduler.NextWake([Event(Periodicity.SingleShot)], Start));
    }

    [Fact]
    public void Upcoming_ListsOccurrencesInOrderUpToTheLimit()
    {
        ScheduledEvent[] events = [Event(Periodicity.Day, description: "d"), Event(Periodicity.Hour, description: "h")];

        var upcoming = EventScheduler.Upcoming(events, Start, 25);

        Assert.Equal(25, upcoming.Count);
        Assert.Equal(Start.AddHours(1), upcoming[0].Occurrence);
        Assert.True(upcoming.Zip(upcoming.Skip(1)).All(p => p.First.Occurrence <= p.Second.Occurrence));
        Assert.Contains(upcoming, u => u.Event.Description == "d");
    }

    [Fact]
    public void FiredLog_SurvivesARoundTrip()
    {
        var log = new FiredLog();
        Due(Event(), Start, log);

        var reloaded = FiredLog.Deserialize(log.Serialize());

        Assert.Empty(Due(Event(), Start.AddMinutes(1), reloaded));
    }

    [Fact]
    public void FiredLog_StartsEmpty_WhenItsFileIsCorrupt()
    {
        Assert.Single(Due(Event(), Start, FiredLog.Deserialize("{not json")));
    }
}
