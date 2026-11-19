using LaunchPlanner.Core.Persistence;

namespace LaunchPlanner.Services
{
    public static class AppPaths
    {
#if DEBUG
        private const string ScheduleFileName = "LaunchPlanner schedule_DEBUG.txt";
#else
        private const string ScheduleFileName = "LaunchPlanner schedule.txt";
#endif
        private const string FiredFileName = "LaunchPlanner fired.json";

        public static IEventRepository CreateRepository()
        {
            Directory.CreateDirectory(DataFolder());
            return new FileEventRepository(
                Path.Combine(DataFolder(), ScheduleFileName),
                Path.Combine(FileSystem.AppDataDirectory, FiredFileName));
        }

        // Su Windows è la stessa cartella di LaunchPlanner_Forms, così il file degli eventi resta quello
        private static string DataFolder()
        {
#if WINDOWS
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LaunchPlanner");
#else
            return FileSystem.AppDataDirectory;
#endif
        }
    }
}
