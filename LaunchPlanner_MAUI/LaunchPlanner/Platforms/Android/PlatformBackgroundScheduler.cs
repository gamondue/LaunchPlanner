using Android.App;
using Android.Content;
using Android.Provider;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Scheduling;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    // La parte residente è una sveglia di AlarmManager: il sistema la conserva anche a processo
    // terminato e la consegna a EventAlarmReceiver, che ricalcola la sveglia successiva
    public sealed class PlatformBackgroundScheduler(EventStore store, FiringService firing, ActivityLog log) : IBackgroundScheduler
    {
        private const int AlarmRequestCode = 1;
        private const string OverlayAskedKey = "overlayAsked";
        private const string StoppedKey = "stoppedByUser";

        // Senza sveglie esatte, o in Doze, Android può consegnare la sveglia con diversi minuti di ritardo
        private static readonly TimeSpan LateAlarmWindow = TimeSpan.FromMinutes(15);

        private bool arePermissionsRequested;
        private bool hasProcessStarted;

        public void Start()
        {
            Preferences.Default.Set(StoppedKey, false);
            _ = RunAsync(requestPermissions: true);
        }

        // Avvio del processo (anche dal riavvio del telefono): rilancia gli eventi in corso
        public async Task RunAtProcessStartAsync()
        {
            if (hasProcessStarted)
                return;

            hasProcessStarted = true;
            try
            {
                await firing.RunStartupAsync(DateTime.Now, LateAlarmWindow);
            }
            catch (Exception ex)
            {
                log.AddError(string.Format(Strings.Error_EventCheckFailed, ex.Message));
            }
        }

        // Uscita voluta dall'utente: toglie la sveglia e fa ignorare i riarmi di sistema
        // (avvio del telefono, cambio d'orologio) finché l'app non viene riaperta a mano
        public void Stop()
        {
            Preferences.Default.Set(StoppedKey, true);
            var (alarms, alarm) = GetAlarm();
            alarms.Cancel(alarm);
        }

        private static (AlarmManager Alarms, PendingIntent Alarm) GetAlarm()
        {
            var context = Android.App.Application.Context;
            var alarms = (AlarmManager)context.GetSystemService(Context.AlarmService)!;
            var alarm = PendingIntent.GetBroadcast(
                context, AlarmRequestCode, new Intent(context, typeof(EventAlarmReceiver)),
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
            return (alarms, alarm);
        }

        public void Rearm()
        {
            store.EnsureLoaded();
            var (alarms, alarm) = GetAlarm();

            alarms.Cancel(alarm);
            if (EventScheduler.NextWake(store.Events, DateTime.Now) is not { } wake)
                return;

            var triggerAt = new DateTimeOffset(wake).ToUnixTimeMilliseconds();
            if (!OperatingSystem.IsAndroidVersionAtLeast(31) || alarms.CanScheduleExactAlarms())
                alarms.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAt, alarm);
            else
                alarms.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAt, alarm);
        }

        // Lancia ciò che è in scadenza e arma la sveglia successiva
        public async Task RunAsync(bool requestPermissions = false)
        {
            try
            {
                if (!requestPermissions && Preferences.Default.Get(StoppedKey, false))
                    return;
                if (requestPermissions)
                    await RequestPermissionsOnceAsync();
                if (requestPermissions)
                    await RunAtProcessStartAsync();
                await firing.RunDueAsync(DateTime.Now, LateAlarmWindow);
                Rearm();
            }
            catch (Exception ex)
            {
                log.AddError(string.Format(Strings.Error_EventCheckFailed, ex.Message));
            }
        }

        private async Task RequestPermissionsOnceAsync()
        {
            var activity = Platform.CurrentActivity;
            if (activity is null || arePermissionsRequested)
                return;

            arePermissionsRequested = true;
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                await Permissions.RequestAsync<Permissions.PostNotifications>();

            var alarms = (AlarmManager)activity.GetSystemService(Context.AlarmService)!;
            if (OperatingSystem.IsAndroidVersionAtLeast(31) && !alarms.CanScheduleExactAlarms())
                activity.StartActivity(new Intent(Settings.ActionRequestScheduleExactAlarm));

            // Senza questo permesso Android non consente di aprire il link da background: resta la notifica
            if (!Settings.CanDrawOverlays(activity) && !Preferences.Default.Get(OverlayAskedKey, false))
            {
                Preferences.Default.Set(OverlayAskedKey, true);
                activity.StartActivity(new Intent(
                    Settings.ActionManageOverlayPermission, Android.Net.Uri.Parse($"package:{activity.PackageName}")));
            }
        }
    }
}
