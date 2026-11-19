using LaunchPlanner.Core.Models;

namespace LaunchPlanner.Core.Scheduling;

public sealed record EventOccurrence(ScheduledEvent Event, DateTime Occurrence);

public static class EventScheduler
{
    // Margine minimo per lanciare un evento: le sveglie dei sistemi mobili possono ritardare
    // e con durata zero l'evento andrebbe perso
    public static readonly TimeSpan MinimumWindow = TimeSpan.FromMinutes(1);

    // Restituisce gli eventi da lanciare adesso e li segna come lanciati
    public static IReadOnlyList<EventOccurrence> TakeDue(
        IEnumerable<ScheduledEvent> events, DateTime now, FiredLog fired, TimeSpan? minimumWindow = null)
    {
        var shortestWindow = minimumWindow ?? MinimumWindow;
        var due = new List<EventOccurrence>();
        foreach (var ev in events.Where(ev => ev.Enabled && now <= ev.Finish))
        {
            if (OccurrenceCalculator.Latest(ev, now) is not { } occurrence)
                continue;

            var window = ev.Duration > shortestWindow ? ev.Duration : shortestWindow;
            var isStillRunning = fired.LastFired(ev) is { } last && occurrence <= last + ev.Duration;
            if (now <= occurrence + window && !isStillRunning)
            {
                fired.Mark(ev, occurrence);
                due.Add(new EventOccurrence(ev, occurrence));
            }
        }
        return due;
    }

    public static DateTime? NextWake(IEnumerable<ScheduledEvent> events, DateTime now)
        => events.Where(ev => ev.Enabled).Select(ev => OccurrenceCalculator.Next(ev, now)).Min();

    // Prossime occorrenze in ordine di tempo, per i sistemi che le programmano in anticipo
    public static IReadOnlyList<EventOccurrence> Upcoming(IEnumerable<ScheduledEvent> events, DateTime now, int limit)
        => events.Where(ev => ev.Enabled)
            .SelectMany(ev => OccurrencesOf(ev, now).Take(limit))
            .OrderBy(upcoming => upcoming.Occurrence)
            .Take(limit)
            .ToList();

    private static IEnumerable<EventOccurrence> OccurrencesOf(ScheduledEvent ev, DateTime now)
    {
        for (var next = OccurrenceCalculator.Next(ev, now); next is { } occurrence; next = OccurrenceCalculator.Next(ev, occurrence))
            yield return new EventOccurrence(ev, occurrence);
    }
}
