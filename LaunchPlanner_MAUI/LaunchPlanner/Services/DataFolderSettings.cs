using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    // Solo su Windows l'utente può scegliere la cartella: su Android e iOS lo spazio del programma
    // è privato e il selettore di cartelle restituisce URI, non percorsi utilizzabili dal file system
    public sealed class DataFolderSettings : IDataFolderSettings
    {
#if WINDOWS
        public bool IsSupported => true;
#else
        public bool IsSupported => false;
#endif

        public string DefaultFolder => AppPaths.DefaultFolder();

        public string SelectedFolder => AppPaths.SelectedFolder();

        public async Task<string?> PickFolderAsync()
        {
#if WINDOWS
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");

            var window = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (window is null) return null;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));

            var folder = await picker.PickSingleFolderAsync();
            return folder?.Path;
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        public void Select(string? folder)
        {
            var target = string.IsNullOrWhiteSpace(folder) ? DefaultFolder : folder;
            Directory.CreateDirectory(target);

            // Il file degli eventi segue l'utente, senza sovrascrivere uno già presente nella nuova cartella
            var source = Path.Combine(SelectedFolder, AppPaths.ScheduleFileName);
            var destination = Path.Combine(target, AppPaths.ScheduleFileName);
            if (File.Exists(source) && !File.Exists(destination))
                File.Copy(source, destination);

            AppPaths.StoreSelectedFolder(string.IsNullOrWhiteSpace(folder) ? null : folder);
        }
    }
}
