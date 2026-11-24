#if IOS || MACCATALYST
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    // Le notifiche locali già pianificate sopravvivono al riavvio da sole: non c'è nulla da scegliere
    public sealed class PlatformStartupSettings : IStartupSettings
    {
        public bool IsSupported => false;

        public string Caption => string.Empty;

        public bool IsEnabled
        {
            get => false;
            set { }
        }
    }
}
#endif
