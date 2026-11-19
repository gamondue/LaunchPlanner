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
        public override async void OnReceive(Context? context, Intent? intent)
        {
            var pending = GoAsync();
            try
            {
                var scheduler = IPlatformApplication.Current!.Services.GetRequiredService<IBackgroundScheduler>();
                await ((PlatformBackgroundScheduler)scheduler).RunAsync();
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
