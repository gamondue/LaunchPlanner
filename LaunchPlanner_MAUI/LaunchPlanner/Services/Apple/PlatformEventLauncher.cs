#if IOS || MACCATALYST
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    public sealed class PlatformEventLauncher : IEventLauncher
    {
        public async Task LaunchAsync(ScheduledEvent ev)
        {
            var target = LaunchTarget.Resolve(ev.Url)!;
            var isOpened = File.Exists(target)
                ? await Launcher.Default.OpenAsync(new OpenFileRequest(ev.Description, new ReadOnlyFile(target)))
                : await Launcher.Default.OpenAsync(new Uri(target));

            if (!isOpened)
                throw new InvalidOperationException(string.Format(Strings.Error_NoAppCanOpen, target));
        }
    }
}
#endif
