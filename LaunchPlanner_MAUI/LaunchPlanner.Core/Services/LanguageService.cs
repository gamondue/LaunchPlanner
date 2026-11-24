using System.Globalization;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.Services;

// Lingua dell'interfaccia scelta dall'utente. Agisce su Strings.Culture e non sulla cultura dei thread:
// vale per tutti i thread e lascia le date nel formato regionale dell'utente.
public sealed class LanguageService(ILanguageStore store)
{
    public const string English = "en"; // la lingua del primo avvio e di ogni scelta non valida
    public const string Italian = "it";

    // Ogni lingua è scritta nel proprio nome, qualunque sia la lingua in uso
    public IReadOnlyList<LanguageOption> Options =>
    [
        new(English, "English"),
        new(Italian, "Italiano"),
    ];

    public string Current => Strings.Culture?.Name ?? English;

    // Dopo un cambio l'interfaccia già creata va ricostruita: la XAML legge i testi una volta sola
    public event Action? Changed;

    public void Apply()
    {
        var saved = store.Language;
        Strings.Culture = CultureInfo.GetCultureInfo(saved == Italian ? Italian : English);
    }

    public void Change(string language)
    {
        if (language == Current)
            return;

        store.Language = language;
        Apply();
        Changed?.Invoke();
    }
}
