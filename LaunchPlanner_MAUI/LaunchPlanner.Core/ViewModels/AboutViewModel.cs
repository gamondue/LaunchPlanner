using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Core.ViewModels;

public sealed class AboutViewModel(string version, LanguageService language)
{
    public IReadOnlyList<LanguageOption> Languages => language.Options;

    public LanguageOption? SelectedLanguage
    {
        get => Languages.First(option => option.Code == language.Current);
        set
        {
            if (value is not null)
                language.Change(value.Code);
        }
    }

    public string Title => "LaunchPlanner";
    public string Summary => Strings.About_Summary;
    public string Disclaimer => Strings.About_Disclaimer;
    public string Version => string.Format(Strings.About_Version, version);
    public string Author => "Ing. Gabriele MONTI";
    public string City => "Forlì";
    public string Country => Strings.About_Country;
    public string Email => "ing@ingmonti.it";
}
