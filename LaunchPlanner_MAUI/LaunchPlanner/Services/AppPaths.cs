using LaunchPlanner.Core.Persistence;

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

        public static IEventRepository CreateRepository()
        {
            var folder = SelectedFolder();
            Directory.CreateDirectory(folder);
            return new FileEventRepository(
                Path.Combine(folder, ScheduleFileName),
                Path.Combine(FileSystem.AppDataDirectory, FiredFileName));
        }

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
