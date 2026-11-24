namespace LaunchPlanner
{
    public partial class AppShell : Shell
    {
        public const string LogRoute = "//log";
        public const string EventsRoute = "//events";
        public const string AboutRoute = "//about";

        public AppShell()
        {
            InitializeComponent();
        }

        public void StartOnAbout() => Tabs.CurrentItem = AboutTab;

        public void StartOnSettings() => Tabs.CurrentItem = SettingsTab;
    }
}
