using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    public sealed class PreferencesLanguageStore : ILanguageStore
    {
        private const string LanguageKey = "language";

        public string Language
        {
            get => Preferences.Default.Get(LanguageKey, LanguageService.English);
            set => Preferences.Default.Set(LanguageKey, value);
        }
    }
}
