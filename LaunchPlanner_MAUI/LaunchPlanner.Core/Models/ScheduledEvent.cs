namespace LaunchPlanner.Core.Models;

public sealed record ScheduledEvent
{
    // Prima di questo istante l'evento non viene mai lanciato. È anche il riferimento delle ripetizioni.
    public DateTime Start { get; init; }

    // Dopo questo istante l'evento non viene lanciato, anche se periodico
    public DateTime Finish { get; init; }

    // Finestra di attivazione: se il programma parte in questo intervallo l'evento viene lanciato.
    // Solo dopo la sua fine l'evento può essere rilanciato.
    public TimeSpan Duration { get; init; }

    public Periodicity Period { get; init; }

    public string Description { get; init; } = "";

    public string Url { get; init; } = "";

    public bool Enabled { get; init; }
}
