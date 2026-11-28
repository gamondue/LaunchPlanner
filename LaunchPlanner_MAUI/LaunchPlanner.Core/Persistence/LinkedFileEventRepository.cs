using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Scheduling;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Core.Persistence;

// Un file scelto dall'utente con il selettore di sistema (OneDrive, Google Drive...): non ha un percorso,
// si può solo leggere e riscrivere per intero. Lancia IOException o UnauthorizedAccessException
public interface IScheduleDocument
{
    string Name { get; }
    string ReadText();
    void WriteText(string text);
}

// Il file di riferimento è il documento; la copia locale serve quando non si riesce a leggerlo
public sealed class LinkedFileEventRepository(
    FileEventRepository cache, IScheduleDocument document, ActivityLog log) : IEventRepository
{
    public string SchedulePath => document.Name;

    // Un file vuoto va bene; per gli altri serve almeno un evento valido, o la sola intestazione.
    // Evita che il primo salvataggio sovrascriva un file scelto per sbaglio
    public static bool IsScheduleText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return true;

        var result = ScheduleFile.Parse(text);
        var header = text.ReplaceLineEndings("\n").Split('\n')[0];
        return result.Events.Count > 0 || (result.Errors.Count == 0 && header.Contains('\t'));
    }

    public ScheduleParseResult Load()
    {
        try
        {
            cache.Mirror(document.ReadText());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException && File.Exists(cache.SchedulePath))
        {
            log.AddError(string.Format(Strings.Error_CloudFileUnreadable, document.Name, ex.Message));
        }
        return cache.Load();
    }

    // Prima il documento: se non si riesce a scriverlo, le modifiche restano "da salvare"
    // e la copia locale non diverge da quella nel cloud
    public void Save(IReadOnlyList<ScheduledEvent> events)
    {
        document.WriteText(ScheduleFile.Format(events));
        cache.Save(events);
    }

    public FiredLog LoadFired() => cache.LoadFired();

    public void SaveFired(FiredLog fired) => cache.SaveFired(fired);
}
