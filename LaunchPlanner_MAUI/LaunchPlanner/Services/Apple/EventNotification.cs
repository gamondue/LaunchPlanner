#if IOS || MACCATALYST
using Foundation;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Scheduling;
using LaunchPlanner.Core.Services;
using UserNotifications;

namespace LaunchPlanner.Services
{
    // L'evento viaggia dentro la notifica, perché quando arriva l'app può essere stata riavviata
    public static class EventNotification
    {
        private const string StartKey = "start";
        private const string OccurrenceKey = "occurrence";
        private const string DescriptionKey = "description";
        private const string UrlKey = "url";

        public static UNNotificationContent CreateContent(EventOccurrence upcoming) => new UNMutableNotificationContent
        {
            Title = ActivityLog.DefaultStatus,
            Body = upcoming.Event.Description,
            Sound = UNNotificationSound.Default,
            UserInfo = NSDictionary.FromObjectsAndKeys(
                new NSObject[]
                {
                    NSNumber.FromInt64(upcoming.Event.Start.Ticks),
                    NSNumber.FromInt64(upcoming.Occurrence.Ticks),
                    new NSString(upcoming.Event.Description),
                    new NSString(upcoming.Event.Url),
                },
                new NSObject[]
                {
                    new NSString(StartKey),
                    new NSString(OccurrenceKey),
                    new NSString(DescriptionKey),
                    new NSString(UrlKey),
                }),
        };

        public static EventOccurrence Read(NSDictionary userInfo)
        {
            var ev = new ScheduledEvent
            {
                Start = new DateTime(((NSNumber)userInfo[StartKey]!).Int64Value),
                Description = userInfo[DescriptionKey]!.ToString()!,
                Url = userInfo[UrlKey]!.ToString()!,
            };
            return new EventOccurrence(ev, new DateTime(((NSNumber)userInfo[OccurrenceKey]!).Int64Value));
        }
    }
}
#endif
