using Android.App;
using Android.Content;
using Android.Provider;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    public sealed class PlatformEventLauncher : IEventLauncher
    {
        private const string ChannelId = "launchplanner_events";

        public Task LaunchAsync(ScheduledEvent ev)
        {
            var context = Android.App.Application.Context;
            var target = Android.Net.Uri.Parse(LaunchTarget.Resolve(ev.Url));
            var intent = new Intent(Intent.ActionView, target).AddFlags(ActivityFlags.NewTask);

            if (IsInForeground())
            {
                context.StartActivity(intent);
                return Task.CompletedTask;
            }

            // Da Android 10 un'app in background non può aprire attività, e il sistema lo impedisce
            // senza segnalare errori: la notifica garantisce il link anche se il tentativo diretto fallisce
            Notify(context, ev, intent);
            if (Settings.CanDrawOverlays(context))
                context.StartActivity(intent);
            return Task.CompletedTask;
        }

        private static bool IsInForeground()
        {
            var state = new ActivityManager.RunningAppProcessInfo();
            ActivityManager.GetMyMemoryState(state);
            return state.Importance == Importance.Foreground;
        }

        private static void Notify(Context context, ScheduledEvent ev, Intent intent)
        {
            var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
            if (!manager.AreNotificationsEnabled())
                throw new InvalidOperationException(Strings.Error_NotificationsDisabled);

            manager.CreateNotificationChannel(new NotificationChannel(ChannelId, Strings.Notification_ChannelName, NotificationImportance.High));

            // Stabile tra un processo e l'altro, a differenza di GetHashCode sulle stringhe
            var id = (ev.Description + ev.Url).Aggregate(17, (hash, letter) => unchecked(hash * 31 + letter));
            var open = PendingIntent.GetActivity(context, id, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            var notification = new Notification.Builder(context, ChannelId)
                .SetSmallIcon(context.ApplicationInfo!.Icon)
                .SetContentTitle(ActivityLog.DefaultStatus)
                .SetContentText(ev.Description)
                .SetContentIntent(open)
                .SetFullScreenIntent(open, true)
                .SetAutoCancel(true)
                .Build();
            manager.Notify(id, notification);
        }
    }
}
