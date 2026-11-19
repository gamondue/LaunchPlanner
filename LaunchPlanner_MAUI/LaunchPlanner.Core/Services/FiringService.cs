using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Scheduling;

namespace LaunchPlanner.Core.Services;

public sealed class FiringService
{
    private readonly EventStore store;
    private readonly IEventRepository repository;
    private readonly IEventLauncher launcher;
    private readonly ActivityLog log;
    private FiredLog? fired;

    public FiringService(EventStore store, IEventRepository repository, IEventLauncher launcher, ActivityLog log)
    {
        this.store = store;
        this.repository = repository;
        this.launcher = launcher;
        this.log = log;
        store.Edited += (before, after) => Fired().Transfer(before, after);
    }

    // minimumWindow allarga il margine dove la sveglia del sistema può arrivare in ritardo
    public async Task RunDueAsync(DateTime now, TimeSpan? minimumWindow = null)
    {
        store.EnsureLoaded();
        var state = Fired();

        var due = EventScheduler.TakeDue(store.Events, now, state, minimumWindow);
        if (due.Count == 0)
            return;

        state.Retain(store.Events);
        SaveFired(state);
        foreach (var item in due)
            await LaunchAsync(item.Event);
    }

    // Per i sistemi che consegnano da soli l'occorrenza, senza passare da RunDueAsync
    public async Task LaunchOccurrenceAsync(ScheduledEvent ev, DateTime occurrence)
    {
        var state = Fired();
        if (state.LastFired(ev) >= occurrence)
            return;

        state.Mark(ev, occurrence);
        SaveFired(state);
        await LaunchAsync(ev);
    }

    private FiredLog Fired()
    {
        if (fired is not null)
            return fired;

        try
        {
            fired = repository.LoadFired();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log.Add(string.Format(Strings.Error_FiredUnreadable, ex.Message));
            fired = new FiredLog();
        }
        return fired;
    }

    private async Task LaunchAsync(ScheduledEvent ev)
    {
        if (LaunchTarget.Resolve(ev.Url) is null)
        {
            log.AddError(string.Format(Strings.Error_MissingUrl, ev.Description));
            return;
        }

        try
        {
            await launcher.LaunchAsync(ev);
            log.Status = $"{ActivityLog.DefaultStatus} ({ev.Description})";
            log.Add(ev.Description);
        }
        catch (Exception ex)
        {
            log.AddError(string.Format(Strings.Error_UrlNotFound, ev.Url, ex.Message));
        }
    }

    private void SaveFired(FiredLog state)
    {
        try
        {
            repository.SaveFired(state);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log.Add(string.Format(Strings.Error_FiredNotSaved, ex.Message));
        }
    }
}
