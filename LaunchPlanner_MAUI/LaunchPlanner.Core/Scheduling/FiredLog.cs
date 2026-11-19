using System.Text.Json;
using System.Text.Json.Serialization;
using LaunchPlanner.Core.Models;

namespace LaunchPlanner.Core.Scheduling;

// Ultima occorrenza lanciata per ogni evento. Viene salvata su file perché
// su Android il processo termina tra una sveglia e l'altra.
public sealed class FiredLog
{
    private readonly Dictionary<string, DateTime> lastFired;

    public FiredLog() : this([]) { }

    private FiredLog(Dictionary<string, DateTime> lastFired) => this.lastFired = lastFired;

    public DateTime? LastFired(ScheduledEvent ev)
        => lastFired.TryGetValue(KeyOf(ev), out var occurrence) ? occurrence : null;

    public void Mark(ScheduledEvent ev, DateTime occurrence) => lastFired[KeyOf(ev)] = occurrence;

    // La chiave dipende dai campi dell'evento: modificandolo non deve risultare mai lanciato
    public void Transfer(ScheduledEvent from, ScheduledEvent to)
    {
        if (lastFired.Remove(KeyOf(from), out var occurrence))
            lastFired[KeyOf(to)] = occurrence;
    }

    public void Retain(IEnumerable<ScheduledEvent> events)
    {
        var alive = events.Select(KeyOf).ToHashSet();
        foreach (var key in lastFired.Keys.Where(key => !alive.Contains(key)).ToList())
            lastFired.Remove(key);
    }

    public string Serialize() => JsonSerializer.Serialize(lastFired, FiredLogJson.Default.DictionaryStringDateTime);

    public static FiredLog Deserialize(string json)
    {
        try
        {
            return new FiredLog(JsonSerializer.Deserialize(json, FiredLogJson.Default.DictionaryStringDateTime) ?? []);
        }
        catch (JsonException)
        {
            return new FiredLog();
        }
    }

    private static string KeyOf(ScheduledEvent ev) => $"{ev.Start:s}|{ev.Description}|{ev.Url}";
}

[JsonSerializable(typeof(Dictionary<string, DateTime>))]
internal sealed partial class FiredLogJson : JsonSerializerContext;
