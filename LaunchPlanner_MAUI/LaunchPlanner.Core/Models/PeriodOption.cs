using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.Models;

public sealed record PeriodOption(Periodicity Period)
{
    public static IReadOnlyList<PeriodOption> All { get; } =
    [
        new(Periodicity.Custom),
        new(Periodicity.SingleShot),
        new(Periodicity.Second),
        new(Periodicity.OneMinute),
        new(Periodicity.QuarterOfHour),
        new(Periodicity.HalfHour),
        new(Periodicity.Hour),
        new(Periodicity.Day),
        new(Periodicity.Weekly),
        new(Periodicity.Weekday),
        new(Periodicity.Weekend),
        new(Periodicity.Monday),
        new(Periodicity.Tuesday),
        new(Periodicity.Wednesday),
        new(Periodicity.Thursday),
        new(Periodicity.Friday),
        new(Periodicity.Saturday),
        new(Periodicity.Sunday),
        new(Periodicity.Month),
        new(Periodicity.Year),
        new(Periodicity.NonPeriodical),
    ];

    // Letta ogni volta: la lingua può cambiare mentre il programma è in esecuzione
    public string Caption => Strings.ResourceManager.GetString($"Period_{Period}", Strings.Culture)!;

    public override string ToString() => Caption;
}
