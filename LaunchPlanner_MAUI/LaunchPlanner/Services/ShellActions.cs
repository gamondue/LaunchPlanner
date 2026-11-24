using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    public sealed class ShellActions(
        IEventRepository repository, IEventLauncher launcher, ActivityLog log) : IShellActions
    {
        public bool CanEditFile => DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.MacCatalyst;

        public bool CanExit => DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.Android;

        public void CloseEditor()
        {
#if WINDOWS
            IPlatformApplication.Current!.Services.GetRequiredService<WindowHost>().Hide();
#endif
            _ = Shell.Current.GoToAsync(AppShell.LogRoute);
        }

        public async void OpenScheduleFile()
        {
            try
            {
                await launcher.LaunchAsync(new ScheduledEvent { Url = repository.SchedulePath });
            }
            catch (Exception ex)
            {
                log.AddError(string.Format(Strings.Error_CannotOpenSchedule, repository.SchedulePath, ex.Message));
            }
        }

        public void Exit()
        {
#if WINDOWS
            IPlatformApplication.Current!.Services.GetRequiredService<TrayIcon>().Dispose();
            IPlatformApplication.Current!.Services.GetRequiredService<WindowHost>().Exit();
#elif ANDROID
            var scheduler = (PlatformBackgroundScheduler)IPlatformApplication.Current!.Services.GetRequiredService<IBackgroundScheduler>();
            scheduler.Stop();
            Platform.CurrentActivity?.FinishAndRemoveTask();
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
        }
    }
}
