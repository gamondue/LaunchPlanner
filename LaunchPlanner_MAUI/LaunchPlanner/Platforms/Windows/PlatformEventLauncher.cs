using System.Diagnostics;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    public sealed class PlatformEventLauncher : IEventLauncher
    {
        public Task LaunchAsync(ScheduledEvent ev)
        {
            // UseShellExecute apre il documento o l'URL con l'applicazione associata
            Process.Start(new ProcessStartInfo
            {
                FileName = LaunchTarget.Resolve(ev.Url)!,
                UseShellExecute = true,
            })?.Dispose();
            return Task.CompletedTask;
        }
    }
}
