#if IOS || MACCATALYST
using Foundation;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Scheduling;
using LaunchPlanner.Core.Services;
using UserNotifications;

namespace LaunchPlanner.Services
{
    // iOS non permette codice residente. La parte che sopravvive alla chiusura è l'elenco delle
    // prossime notifiche locali, che il sistema consegna da solo anche ad app terminata.
    public sealed class PlatformBackgroundScheduler(EventStore store, FiringService firing, ActivityLog log) : IBackgroundScheduler
    {
        // Limite imposto dal sistema alle notifiche locali in attesa
        private const int MaximumPending = 64;

        public void Start() => _ = RunAsync();

        public void Rearm()
        {
            store.EnsureLoaded();
            var center = UNUserNotificationCenter.Current;
            center.RemoveAllPendingNotificationRequests();

            var upcoming = EventScheduler.Upcoming(store.Events, DateTime.Now, MaximumPending);
            for (var i = 0; i < upcoming.Count; i++)
            {
                var request = UNNotificationRequest.FromIdentifier(
                    i.ToString(), EventNotification.CreateContent(upcoming[i]), Trigger(upcoming[i].Occurrence));
                center.AddNotificationRequest(request, error =>
                {
                    if (error is not null)
                        MainThread.BeginInvokeOnMainThread(() => log.Add(string.Format(Strings.Error_NotificationNotScheduled, error.LocalizedDescription)));
                });
            }
        }

        private async Task RunAsync()
        {
            try
            {
                var authorization = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
                    UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound);
                if (!authorization.Item1)
                    log.AddError(Strings.Error_NotificationsDenied);
                await firing.RunDueAsync(DateTime.Now);
                Rearm();
            }
            catch (Exception ex)
            {
                log.AddError(string.Format(Strings.Error_EventCheckFailed, ex.Message));
            }
        }

        private static UNCalendarNotificationTrigger Trigger(DateTime occurrence)
            => UNCalendarNotificationTrigger.CreateTrigger(
                new NSDateComponents
                {
                    Year = occurrence.Year,
                    Month = occurrence.Month,
                    Day = occurrence.Day,
                    Hour = occurrence.Hour,
                    Minute = occurrence.Minute,
                    Second = occurrence.Second,
                },
                repeats: false);
    }
}
#endif
