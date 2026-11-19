using CommunityToolkit.Mvvm.Input;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Core.ViewModels;

public sealed partial class LogViewModel(
    EventStore store, ActivityLog log, IBackgroundScheduler background, IShellActions shell)
{
    public ActivityLog Log => log;
    public bool CanEditFile => shell.CanEditFile;
    public bool CanExit => shell.CanExit;

    [RelayCommand]
    private void Reload()
    {
        store.Reload();
        background.Rearm();
    }

    [RelayCommand]
    private void EditFile() => shell.OpenScheduleFile();

    [RelayCommand]
    private void Exit() => shell.Exit();
}
