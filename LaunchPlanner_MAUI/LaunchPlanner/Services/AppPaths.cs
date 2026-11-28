using System.Security.Cryptography;
using System.Text;
using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    public static class AppPaths
    {
#if DEBUG
        public const string ScheduleFileName = "LaunchPlanner schedule_DEBUG.txt";
#else
        public const string ScheduleFileName = "LaunchPlanner schedule.txt";
#endif
        private const string FiredFileName = "LaunchPlanner fired.json";
        private const string DataFolderKey = "DataFolder";

        private const string CloudUriKey = "CloudFileUri";
        private const string CloudNameKey = "CloudFileName";

        private static string FiredPath => Path.Combine(FileSystem.AppDataDirectory, FiredFileName);

        public static IEventRepository CreateRepository(ActivityLog log)
        {
            if (CloudFile() is { } cloud)
                return new LinkedFileEventRepository(CloudCache(cloud.Uri), OpenCloudDocument(cloud.Uri, cloud.Name), log);

#if ANDROID

            // Se l'utente ha revocato il permesso sulla cartella Download si torna allo spazio privato,
            // dimenticando la scelta perché la pagina delle impostazioni mostri la cartella davvero in uso
            if (!StorageAccess.CanUse(SelectedFolder()))
                StoreSelectedFolder(null);
#endif
            var folder = SelectedFolder();
            Directory.CreateDirectory(folder);
            return new FileEventRepository(Path.Combine(folder, ScheduleFileName), FiredPath);
        }

        // Il file nel cloud scelto dall'utente, o null se il file degli eventi sta in una cartella
        public static (string Uri, string Name)? CloudFile()
        {
            var uri = Preferences.Default.Get(CloudUriKey, "");
            return uri.Length == 0 ? null : (uri, Preferences.Default.Get(CloudNameKey, uri));
        }

        // La copia locale ha un nome per ogni file nel cloud, così quella di uno non finisce mai in un altro
        public static FileEventRepository CloudCache(string uri)
        {
            var id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(uri)))[..16];
            Directory.CreateDirectory(FileSystem.AppDataDirectory);
            return new FileEventRepository(Path.Combine(FileSystem.AppDataDirectory, $"cloud {id}.txt"), FiredPath);
        }

        // Il file degli eventi sul dispositivo: per un file nel cloud è la sua copia locale
        public static string CurrentSchedulePath() =>
            CloudFile() is { } cloud
                ? CloudCache(cloud.Uri).SchedulePath
                : Path.Combine(SelectedFolder(), ScheduleFileName);

        public static void StoreCloudFile(string uri, string name)
        {
            Preferences.Default.Set(CloudUriKey, uri);
            Preferences.Default.Set(CloudNameKey, name);
        }

        public static void ForgetCloudFile()
        {
            Preferences.Default.Remove(CloudUriKey);
            Preferences.Default.Remove(CloudNameKey);
        }

        // L'indirizzo è un URI su Android, il percorso del file altrove
        private static IScheduleDocument OpenCloudDocument(string address, string name) =>
#if ANDROID
            new AndroidScheduleDocument(Android.Net.Uri.Parse(address)!, name);
#else
            new FileScheduleDocument(address);
#endif

        // Su Windows è la stessa cartella di LaunchPlanner_Forms, così il file degli eventi resta quello
        public static string DefaultFolder()
        {
#if WINDOWS
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LaunchPlanner");
#else
            return FileSystem.AppDataDirectory;
#endif
        }

        // La cartella scelta dall'utente, o quella predefinita se non ne ha scelta una
        public static string SelectedFolder()
        {
            var chosen = Preferences.Default.Get(DataFolderKey, "");
            return string.IsNullOrWhiteSpace(chosen) ? DefaultFolder() : chosen;
        }

        // Null o vuoto ripristina la cartella predefinita
        public static void StoreSelectedFolder(string? folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                Preferences.Default.Remove(DataFolderKey);
            else
                Preferences.Default.Set(DataFolderKey, folder);
        }
    }
}
