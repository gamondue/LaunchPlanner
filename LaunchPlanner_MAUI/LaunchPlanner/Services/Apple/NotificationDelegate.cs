#if IOS || MACCATALYST
using LaunchPlanner.Core.Services;
using UserNotifications;

namespace LaunchPlanner.Services
{
    public sealed class NotificationDelegate : UNUserNotificationCenterDelegate
    {
        // App in primo piano: si può lanciare direttamente l'evento
        public override void WillPresentNotification(
            UNUserNotificationCenter center, UNNotification notification,
            Action<UNNotificationPresentationOptions> completionHandler)
        {
            Launch(notification);
            completionHandler(UNNotificationPresentationOptions.None);
        }

        // App in background o chiusa: l'evento parte quando si tocca la notifica
        public override void DidReceiveNotificationResponse(
            UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            Launch(response.Notification);
            completionHandler();
        }

        private static void Launch(UNNotification notification)
        {
            var delivered = EventNotification.Read(notification.Request.Content.UserInfo);
            var firing = IPlatformApplication.Current!.Services.GetRequiredService<FiringService>();
            MainThread.BeginInvokeOnMainThread(() => _ = firing.LaunchOccurrenceAsync(delivered.Event, delivered.Occurrence));
        }
    }
}
#endif
