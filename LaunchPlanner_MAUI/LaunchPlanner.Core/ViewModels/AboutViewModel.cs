using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.ViewModels;

public sealed class AboutViewModel(string version)
{
    public string Title => "LaunchPlanner";
    public string Summary => Strings.About_Summary;
    public string Disclaimer => Strings.About_Disclaimer;
    public string Version => string.Format(Strings.About_Version, version);
    public string Author => "Ing. Gabriele MONTI";
    public string City => "Forlì";
    public string Country => Strings.About_Country;
    public string Email => "ing@ingmonti.it";
}
