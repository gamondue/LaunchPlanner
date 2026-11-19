using System.Globalization;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.Services;

// Lingua dell'interfaccia scelta dall'utente. Agisce su Strings.Culture e non sulla cultura dei thread:
// vale per tutti i thread e lascia le date nel formato regionale dell'utente.
public sealed class LanguageService(ILanguageStore store)
{
    public const string SystemLanguage = "";

    private const string Italian = "it";
    private const string English = "en";

    public IReadOnlyList<LanguageOption> Options =>
    [
        new(SystemLanguage, Strings.Language_System),
        new(Italian, "Italiano"),
        new(English, "English"),
    ];

    public string Current => Strings.Culture?.Name ?? SystemLanguage;

    // Dopo un cambio l'interfaccia già creata va ricostruita: la XAML legge i testi una volta sola
    public event Action? Changed;

    public void Apply()
    {
        var saved = store.Language;
        var isTranslated = saved is Italian or English;
        Strings.Culture = isTranslated ? CultureInfo.GetCultureInfo(saved) : null;
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
