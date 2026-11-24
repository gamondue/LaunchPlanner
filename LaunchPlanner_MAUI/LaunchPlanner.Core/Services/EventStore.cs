using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.Services;

// Elenco degli eventi in uso. Le modifiche valgono subito, sul file vanno solo con Save.
public sealed class EventStore(IEventRepository repository, ActivityLog log)
{
    private IReadOnlyList<ScheduledEvent> events = [];
    private bool isLoaded;
    private bool hasLoadFailed;

    public IReadOnlyList<ScheduledEvent> Events => events;
    public bool CanSave => !hasLoadFailed;

    public event Action? Changed;

    public event Action<ScheduledEvent, ScheduledEvent>? Edited;

    public void EnsureLoaded()
    {
        if (!isLoaded)
            Reload();
    }

    public void Reload()
    {
        // Un fallimento vale come tentativo: si ripete a richiesta, non a ogni controllo del timer
        isLoaded = true;
        try
        {
            var result = repository.Load();
            hasLoadFailed = false;
            foreach (var error in result.Errors)
                log.AddError($"{repository.SchedulePath}: {error}");
            Replace(result.Events);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            hasLoadFailed = true;
            log.AddError(string.Format(Strings.Error_ScheduleUnreadable, repository.SchedulePath, ex.Message));
        }
    }

    public void Replace(IEnumerable<ScheduledEvent> newEvents)
    {
        events = [.. newEvents];
        isLoaded = true;
        Changed?.Invoke();
    }

    public void Add(ScheduledEvent ev) => Replace([.. events, ev]);

    public void RemoveAt(int index) => Replace(events.Where((_, i) => i != index));

    // Non notifica Changed: serve a chi sta modificando la riga
    public void Update(int index, ScheduledEvent ev)
    {
        var before = events[index];
        events = events.Select((current, i) => i == index ? ev : current).ToList();
        Edited?.Invoke(before, ev);
    }

    public void Save()
    {
        // L'elenco in memoria non rappresenta il file: salvarlo lo cancellerebbe
        if (hasLoadFailed)
            throw new IOException(string.Format(Strings.Error_ScheduleNotLoaded, repository.SchedulePath));
        repository.Save(events);
    }
}
