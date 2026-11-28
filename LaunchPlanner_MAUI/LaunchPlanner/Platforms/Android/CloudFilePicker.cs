using Android.App;
using Android.Content;
using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Services
{
    // Il selettore di file di sistema, che elenca anche OneDrive, Google Drive e gli altri servizi installati.
    // Non servono permessi nel manifest: l'accesso al file lo concede l'utente scegliendolo
    public static class CloudFilePicker
    {
        private const int RequestCode = 4712;

        private static TaskCompletionSource<Android.Net.Uri?>? pending;

        // Null se l'utente annulla
        public static Task<Android.Net.Uri?> PickAsync()
        {
            var activity = Platform.CurrentActivity ?? throw new IOException(Strings.Error_NoFilePicker);

            // Senza tipo preciso: i servizi non dichiarano tutti lo stesso per un file .txt
            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission
                | ActivityFlags.GrantPersistableUriPermission);

            pending?.TrySetResult(null);
            pending = new TaskCompletionSource<Android.Net.Uri?>();
            try
            {
                activity.StartActivityForResult(intent, RequestCode);
            }
            catch (ActivityNotFoundException)
            {
                throw new IOException(Strings.Error_NoFilePicker);
            }
            return pending.Task;
        }

        // Chiamato da MainActivity quando il selettore si chiude
        public static void OnPicked(int requestCode, Result resultCode, Intent? data)
        {
            if (requestCode != RequestCode) return;
            pending?.TrySetResult(resultCode == Result.Ok ? data?.Data : null);
            pending = null;
        }

        // Senza il permesso permanente l'accesso andrebbe perso al riavvio del telefono
        public static void KeepAccess(Android.Net.Uri uri)
        {
            try
            {
                Android.App.Application.Context.ContentResolver!.TakePersistableUriPermission(
                    uri, ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            }
            catch (Java.Lang.SecurityException)
            {
                throw new UnauthorizedAccessException(Strings.Error_CloudNoLastingAccess);
            }
        }
    }
}
