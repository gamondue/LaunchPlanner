using LaunchPlanner.Core.Services;

namespace LaunchPlanner
{
    public partial class App : Application
    {
        private readonly IBackgroundScheduler background;

        public App(IBackgroundScheduler background, LanguageService language)
        {
            this.background = background;
            language.Changed += RebuildInterface;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
#if WINDOWS
            shell.StartOnAbout();
#endif
            return new Window(shell) { Title = ActivityLog.DefaultStatus };
        }

        // Rimandato: la richiesta arriva dal selettore della pagina che viene sostituita.
        // Si riparte dall'About, dove si trova il selettore della lingua.
        private void RebuildInterface() => Dispatcher.Dispatch(() =>
        {
            foreach (var window in Windows)
            {
                var shell = new AppShell();
                shell.StartOnAbout();
                window.Page = shell;
            }
        });

        protected override void OnStart() => background.Start();

        protected override void OnResume() => background.Start();
    }
}
