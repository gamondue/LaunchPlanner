using Android.App;
using Android.Content;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    // Riceve la sveglia e, poiché le sveglie non sopravvivono al riavvio o a un cambio
    // di orologio, anche gli avvisi di sistema che obbligano a riarmarla
    [BroadcastReceiver(Exported = true)]
    [IntentFilter([
        Intent.ActionBootCompleted,
        Intent.ActionMyPackageReplaced,
        Intent.ActionTimeChanged,
        Intent.ActionTimezoneChanged,
        AlarmManager.ActionScheduleExactAlarmPermissionStateChanged])]
    public sealed class EventAlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            var pending = GoAsync();
            _ = HandleAsync(pending, isBoot: intent?.Action == Intent.ActionBootCompleted);
        }

        private static async Task HandleAsync(PendingResult? pending, bool isBoot)
        {
            try
            {
                var platform = IPlatformApplication.Current;
                if (platform is null)
                    throw new InvalidOperationException("MAUI application is not initialized.");

                // Dopo il riavvio le sveglie tornano solo se l'utente lo ha scelto in Configurazione
                if (isBoot && !platform.Services.GetRequiredService<IStartupSettings>().IsEnabled)
                    return;

                var scheduler = platform.Services.GetRequiredService<IBackgroundScheduler>();
                var android = (PlatformBackgroundScheduler)scheduler;
                if (isBoot)
                    await android.RunAtProcessStartAsync();
                await android.RunAsync(requestPermissions: false);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(nameof(EventAlarmReceiver), ex.ToString());
            }
            finally
            {
                pending?.Finish();
            }
        }
    }
}
