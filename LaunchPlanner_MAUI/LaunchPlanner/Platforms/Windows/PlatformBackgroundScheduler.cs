using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    // Su Windows il programma resta in esecuzione nel tray: basta un controllo periodico
    public sealed class PlatformBackgroundScheduler(FiringService firing, ActivityLog log) : IBackgroundScheduler
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(1);

        private IDispatcherTimer? timer;

        public void Start()
        {
            if (timer is not null)
                return;

            timer = Application.Current!.Dispatcher.CreateTimer();
            timer.Interval = CheckInterval;
            timer.Tick += OnTick;
            timer.Start();
        }

        public void Rearm()
        {
            // Il controllo periodico legge sempre l'elenco aggiornato
        }

        private async void OnTick(object? sender, EventArgs e)
        {
            try
            {
                await firing.RunDueAsync(DateTime.Now);
            }
            catch (Exception ex)
            {
                log.AddError(string.Format(Strings.Error_EventCheckFailed, ex.Message));
            }
        }
    }
}
