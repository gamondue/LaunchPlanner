using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    // Su Android l'unica scelta possibile è se, dopo il riavvio del telefono,
    // EventAlarmReceiver deve riarmare le sveglie senza che l'utente apra l'app
    public sealed class PlatformStartupSettings : IStartupSettings
    {
        private const string RearmAtBootKey = "rearmAtBoot";

        public bool IsSupported => true;

        public string Caption => Strings.Settings_Startup_Android;

        // Acceso di default, come il comportamento precedente alla comparsa dell'opzione
        public bool IsEnabled
        {
            get => Preferences.Default.Get(RearmAtBootKey, true);
            set => Preferences.Default.Set(RearmAtBootKey, value);
        }
    }
}
