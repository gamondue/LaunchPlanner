using LaunchPlanner.Core.Models;

namespace LaunchPlanner.Core.Services;

// Apre l'URL o il file dell'evento nel modo consentito dalla piattaforma
public interface IEventLauncher
{
    Task LaunchAsync(ScheduledEvent ev);
}

// La parte "residente": fa scattare gli eventi anche senza interfaccia utente
public interface IBackgroundScheduler
{
    void Start();

    // Va chiamato dopo ogni modifica salvata degli eventi e dopo ogni lancio
    void Rearm();
}

// La lingua scelta dall'utente, conservata tra un avvio e l'altro
public interface ILanguageStore
{
    string Language { get; set; }
}

public interface IShellActions
{
    bool CanEditFile { get; }
    bool CanExit { get; }
    void CloseEditor();
    void OpenScheduleFile();
    void Exit();
}
