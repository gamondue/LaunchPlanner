using LaunchPlanner.Core.Models;

namespace LaunchPlanner.Core.Services;

// Apre l'URL o il file dell'evento nel modo consentito dalla piattaforma
public interface IEventLauncher
{
    Task LaunchAsync(ScheduledEvent ev);
}

// L'icona del programma che si aprirà allo scatto dell'evento: l'eseguibile stesso,
// oppure il programma associato dal sistema all'estensione del file o allo schema dell'URL
public interface IEventIconProvider
{
    // Immagine PNG, o null se il programma non si può determinare
    byte[]? GetIconPng(string? url);
}

// Per le piattaforme dove l'icona non si può ricavare
public sealed class NoEventIconProvider : IEventIconProvider
{
    public byte[]? GetIconPng(string? url) => null;
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

// L'opzione di avvio automatico, il cui significato dipende dalla piattaforma:
// su Windows lancia il programma al login, su Android riarma le sveglie al riavvio del telefono
public interface IStartupSettings
{
    bool IsSupported { get; }

    // Già nella lingua corrente
    string Caption { get; }

    bool IsEnabled { get; set; }
}

// La cartella dove sta il file degli eventi. Il cambio vale dal prossimo avvio,
// perché il repository viene creato una sola volta all'avvio
public interface IDataFolderSettings
{
    bool IsSupported { get; }

    // La cartella predefinita, usata finché l'utente non ne sceglie un'altra
    string DefaultFolder { get; }

    // La cartella scelta: coincide con DefaultFolder se non ne è stata scelta un'altra
    string SelectedFolder { get; }

    // Null se l'utente annulla la finestra di scelta
    Task<string?> PickFolderAsync();

    // Null torna alla cartella predefinita. Copia il file degli eventi nella nuova cartella
    // se lì non ce n'è già uno; lancia IOException o UnauthorizedAccessException se non riesce
    void Select(string? folder);
}

public interface IShellActions
{
    bool CanEditFile { get; }
    bool CanExit { get; }
    void CloseEditor();
    void OpenScheduleFile();
    void Exit();
}
