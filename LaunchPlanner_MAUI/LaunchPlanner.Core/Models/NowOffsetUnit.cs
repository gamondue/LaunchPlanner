using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.Models;

public enum OffsetUnit
{
    Seconds,
    Minutes,
    Hours,
    Days,
    Months,
    Years,
}

// Un'unità dell'offset usato da "Ora" per calcolare la fine dell'evento
public sealed record NowOffsetUnit(OffsetUnit Unit)
{
    public static IReadOnlyList<NowOffsetUnit> All { get; } =
        [.. Enum.GetValues<OffsetUnit>().Select(unit => new NowOffsetUnit(unit))];

    public static NowOffsetUnit Default { get; } = All.Single(option => option.Unit == OffsetUnit.Months);

    // Letta ogni volta: la lingua può cambiare mentre il programma è in esecuzione
    public string Caption => Strings.ResourceManager.GetString($"Unit_{Unit}", Strings.Culture)!;

    public override string ToString() => Caption;
}
