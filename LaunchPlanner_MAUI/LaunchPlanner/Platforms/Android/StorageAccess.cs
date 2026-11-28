using Android.Content;
using Android.Content.PM;
using AndroidX.Core.Content;
using AndroidSettings = Android.Provider.Settings;
using AndroidEnvironment = Android.OS.Environment;

namespace LaunchPlanner.Services
{
    // L'accesso alla cartella LaunchPlanner dentro la cartella pubblica Download, dove l'utente può mettere o prendere il file degli eventi.
    // Da Android 11 serve "Accesso a tutti i file", perché un file copiato lì da un altro programma o dal PC
    // non appartiene all'app; prima basta il permesso di scrittura nella memoria esterna
    public static class StorageAccess
    {
        public const int AllFilesAccessRequestCode = 4711;

        private static TaskCompletionSource? pendingSettings;

#pragma warning disable CA1422 // ancora il modo più semplice per avere un percorso del file system
        public static string ScheduleFolder => Path.Combine(
            AndroidEnvironment.GetExternalStoragePublicDirectory(AndroidEnvironment.DirectoryDownloads)!.AbsolutePath,
            "LaunchPlanner");

        private static string ExternalRoot => AndroidEnvironment.ExternalStorageDirectory!.AbsolutePath;
#pragma warning restore CA1422

        public static bool HasAccess
        {
            get
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(30))
                    return AndroidEnvironment.IsExternalStorageManager;

                return ContextCompat.CheckSelfPermission(
                    Android.App.Application.Context, Android.Manifest.Permission.WriteExternalStorage) == Permission.Granted;
            }
        }

        // Una cartella della memoria esterna si può usare solo con il permesso; lo spazio privato dell'app sempre
        public static bool CanUse(string folder) =>
            !folder.StartsWith(ExternalRoot, StringComparison.Ordinal) || HasAccess;

        public static async Task<bool> RequestAccessAsync()
        {
            if (HasAccess) return true;

            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                await MainThread.InvokeOnMainThreadAsync(OpenAllFilesAccessSettingsAsync);
                return HasAccess;
            }

            var status = await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.StorageWrite>);
            return status == PermissionStatus.Granted;
        }

        // Chiamato da MainActivity quando l'utente torna dalla pagina delle impostazioni di sistema
        public static void OnSettingsClosed(int requestCode)
        {
            if (requestCode != AllFilesAccessRequestCode) return;
            pendingSettings?.TrySetResult();
            pendingSettings = null;
        }

        // Il permesso non si chiede con un dialogo: si apre la pagina di sistema e si aspetta il ritorno
        private static Task OpenAllFilesAccessSettingsAsync()
        {
            var activity = Platform.CurrentActivity;
            if (activity is null) return Task.CompletedTask;

            pendingSettings?.TrySetResult();
            pendingSettings = new TaskCompletionSource();

            try
            {
                var intent = new Intent(AndroidSettings.ActionManageAppAllFilesAccessPermission,
                    Android.Net.Uri.Parse("package:" + activity.PackageName));
                activity.StartActivityForResult(intent, AllFilesAccessRequestCode);
            }
            catch (ActivityNotFoundException)
            {
                // Alcuni produttori non hanno la pagina per la singola app: si apre l'elenco generale
                activity.StartActivityForResult(
                    new Intent(AndroidSettings.ActionManageAllFilesAccessPermission), AllFilesAccessRequestCode);
            }

            return pendingSettings.Task;
        }
    }
}
