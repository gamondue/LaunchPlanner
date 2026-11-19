using LaunchPlanner.Core.Models;

namespace LaunchPlanner.Core.Scheduling;

// Le occorrenze di un evento sono la successione Start, Start + passo, Start + 2 passi...
// Le date sono locali: le somme conservano l'ora dell'orologio, anche al cambio dell'ora legale.
public static class OccurrenceCalculator
{
    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    // Ultima occorrenza non successiva a now, null se non ce ne sono
    public static DateTime? Latest(ScheduledEvent ev, DateTime now)
    {
        if (now < ev.Start)
            return null;

        var recurrence = RecurrenceOf(ev);
        if (recurrence is null)
            return ev.Start;

        for (var index = recurrence.IndexAtOrBefore(now); index >= 0; index--)
        {
            var occurrence = recurrence.At(index);
            if (recurrence.Allows(occurrence.DayOfWeek))
                return occurrence;
        }
        return null;
    }

    // Prima occorrenza successiva ad after ed entro la fine dell'evento, null se non ce ne sono
    public static DateTime? Next(ScheduledEvent ev, DateTime after)
    {
        var recurrence = RecurrenceOf(ev);
        if (recurrence is null)
            return after < ev.Start && ev.Start <= ev.Finish ? ev.Start : null;

        var index = after < ev.Start ? 0 : recurrence.IndexAtOrBefore(after) + 1;
        for (; ; index++)
        {
            var occurrence = recurrence.At(index);
            if (occurrence > ev.Finish)
                return null;
            if (recurrence.Allows(occurrence.DayOfWeek))
                return occurrence;
        }
    }

    private static Recurrence? RecurrenceOf(ScheduledEvent ev) => ev.Period switch
    {
        Periodicity.Second => Every(ev, TimeSpan.FromSeconds(1)),
        Periodicity.OneMinute => Every(ev, TimeSpan.FromMinutes(1)),
        Periodicity.QuarterOfHour => Every(ev, TimeSpan.FromMinutes(15)),
        Periodicity.HalfHour => Every(ev, TimeSpan.FromMinutes(30)),
        Periodicity.Hour => Every(ev, TimeSpan.FromHours(1)),
        Periodicity.Day => Every(ev, OneDay),
        Periodicity.Weekly => Every(ev, TimeSpan.FromDays(7)),
        Periodicity.Weekday => Every(ev, OneDay, day => day is not (DayOfWeek.Saturday or DayOfWeek.Sunday)),
        Periodicity.Weekend => Every(ev, OneDay, day => day is DayOfWeek.Saturday or DayOfWeek.Sunday),
        Periodicity.Monday => Every(ev, OneDay, day => day == DayOfWeek.Monday),
        Periodicity.Tuesday => Every(ev, OneDay, day => day == DayOfWeek.Tuesday),
        Periodicity.Wednesday => Every(ev, OneDay, day => day == DayOfWeek.Wednesday),
        Periodicity.Thursday => Every(ev, OneDay, day => day == DayOfWeek.Thursday),
        Periodicity.Friday => Every(ev, OneDay, day => day == DayOfWeek.Friday),
        Periodicity.Saturday => Every(ev, OneDay, day => day == DayOfWeek.Saturday),
        Periodicity.Sunday => Every(ev, OneDay, day => day == DayOfWeek.Sunday),
        Periodicity.Month => EveryMonths(ev, 1),
        Periodicity.Year => EveryMonths(ev, 12),
        Periodicity.Custom => CustomRecurrence(ev),
        _ => null,
    };

    // Il periodo personalizzato si ricava dalla durata, come in LaunchPlanner_Forms
    private static Recurrence? CustomRecurrence(ScheduledEvent ev)
    {
        if (ev.Duration <= TimeSpan.FromSeconds(1)) return Every(ev, TimeSpan.FromMinutes(1));
        if (ev.Duration <= TimeSpan.FromMinutes(1)) return Every(ev, TimeSpan.FromHours(1));
        if (ev.Duration <= TimeSpan.FromHours(1)) return Every(ev, OneDay);
        if (ev.Duration <= OneDay) return EveryMonths(ev, 1);
        return null;
    }

    private static Recurrence Every(ScheduledEvent ev, TimeSpan step, Func<DayOfWeek, bool>? allows = null)
        => new FixedRecurrence(ev.Start, step, allows ?? AnyDay);

    private static Recurrence EveryMonths(ScheduledEvent ev, int months)
        => new CalendarRecurrence(ev.Start, months);

    private static bool AnyDay(DayOfWeek day) => true;

    private abstract record Recurrence(DateTime Start, Func<DayOfWeek, bool> Allows)
    {
        public abstract DateTime At(long index);
        public abstract long IndexAtOrBefore(DateTime instant);
    }

    private sealed record FixedRecurrence(DateTime Start, TimeSpan Step, Func<DayOfWeek, bool> Allows)
        : Recurrence(Start, Allows)
    {
        public override DateTime At(long index) => Start + index * Step;

        public override long IndexAtOrBefore(DateTime instant) => (instant - Start).Ticks / Step.Ticks;
    }

    // Parte sempre da Start: il 31 gennaio passa al 28 febbraio ma torna al 31 marzo
    private sealed record CalendarRecurrence(DateTime Start, int Months)
        : Recurrence(Start, AnyDay)
    {
        public override DateTime At(long index) => Start.AddMonths((int)index * Months);

        public override long IndexAtOrBefore(DateTime instant)
        {
            long index = ((instant.Year - Start.Year) * 12 + instant.Month - Start.Month) / Months;
            return At(index) > instant ? index - 1 : index;
        }
    }
}
