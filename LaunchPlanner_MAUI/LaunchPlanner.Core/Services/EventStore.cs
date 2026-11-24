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

    // Vero dopo una modifica non ancora scritta sul file
    public bool HasUnsavedChanges { get; private set; }

    public event Action? Changed;

    public event Action? UnsavedChangesChanged;

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

    // Sostituisce l'elenco con quello letto dal file: non c'è niente da salvare
    public void Replace(IEnumerable<ScheduledEvent> newEvents)
    {
        SetEvents(newEvents);
        SetUnsavedChanges(false);
    }

    public void Add(ScheduledEvent ev)
    {
        SetEvents([.. events, ev]);
        SetUnsavedChanges(true);
    }

    public void RemoveAt(int index)
    {
        SetEvents(events.Where((_, i) => i != index));
        SetUnsavedChanges(true);
    }

    // Non notifica Changed: serve a chi sta modificando la riga
    public void Update(int index, ScheduledEvent ev)
    {
        var before = events[index];
        if (before == ev)
            return;

        events = events.Select((current, i) => i == index ? ev : current).ToList();
        Edited?.Invoke(before, ev);
        SetUnsavedChanges(true);
    }

    public void Save()
    {
        // L'elenco in memoria non rappresenta il file: salvarlo lo cancellerebbe
        if (hasLoadFailed)
            throw new IOException(string.Format(Strings.Error_ScheduleNotLoaded, repository.SchedulePath));
        repository.Save(events);
        SetUnsavedChanges(false);
    }

    private void SetEvents(IEnumerable<ScheduledEvent> newEvents)
    {
        events = [.. newEvents];
        isLoaded = true;
        Changed?.Invoke();
    }

    private void SetUnsavedChanges(bool value)
    {
        if (HasUnsavedChanges == value)
            return;

        HasUnsavedChanges = value;
        UnsavedChangesChanged?.Invoke();
    }
}
