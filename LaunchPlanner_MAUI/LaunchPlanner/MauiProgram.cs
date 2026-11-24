using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Services;
using LaunchPlanner.Core.ViewModels;
using LaunchPlanner.Services;
using LaunchPlanner.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace LaunchPlanner
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services
                .AddSingleton<ILanguageStore, PreferencesLanguageStore>()
                .AddSingleton<LanguageService>()
                .AddSingleton<ActivityLog>()
                .AddSingleton<IEventRepository>(_ => AppPaths.CreateRepository())
                .AddSingleton<EventStore>()
                .AddSingleton<FiringService>()
                .AddSingleton<IEventLauncher, PlatformEventLauncher>()
                .AddSingleton<IBackgroundScheduler, PlatformBackgroundScheduler>()
                .AddSingleton<IStartupSettings, PlatformStartupSettings>()
                .AddSingleton<IDataFolderSettings, DataFolderSettings>()
                .AddSingleton<IShellActions, ShellActions>()
                .AddSingleton<LogViewModel>()
                .AddSingleton<EventsViewModel>()
                .AddSingleton<SettingsViewModel>()
                .AddSingleton(services => new AboutViewModel(
                    AppInfo.Current.VersionString, services.GetRequiredService<LanguageService>()))
                .AddTransient<LogPage>()
                .AddTransient<EventsPage>()
                .AddTransient<SettingsPage>()
                .AddTransient<AboutPage>();

#if WINDOWS
            builder.Services
                .AddSingleton<WindowHost>()
                .AddSingleton<TrayIcon>();
            builder.ConfigureLifecycleEvents(events => events.AddWindows(windows =>
                windows.OnWindowCreated(window =>
                {
                    var services = IPlatformApplication.Current!.Services;
                    services.GetRequiredService<WindowHost>().Attach(window);
                    services.GetRequiredService<TrayIcon>().Show();
                })));
#endif

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Qui passano tutti gli avvii, anche quello di Android per una sveglia senza interfaccia
            var app = builder.Build();
            app.Services.GetRequiredService<LanguageService>().Apply();
            return app;
        }
    }
}
