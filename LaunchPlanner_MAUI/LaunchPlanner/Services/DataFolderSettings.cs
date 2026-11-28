using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    // Su Windows l'utente sceglie una cartella qualsiasi o un singolo file, anche in OneDrive o Google Drive.
    // Su Android il selettore di cartelle restituisce URI, non percorsi del file system, quindi le alternative
    // allo spazio privato sono la cartella Download/LaunchPlanner, accessibile anche dal PC, e un singolo file nel cloud.
    // Su iOS lo spazio del programma resta l'unico
    public sealed class DataFolderSettings : IDataFolderSettings
    {
#if WINDOWS || ANDROID
        public bool IsSupported => true;
#else
        public bool IsSupported => false;
#endif

#if ANDROID
        public string ChooseCaption => Strings.Settings_DataFolder_UseDownload;
#else
        public string ChooseCaption => Strings.Settings_DataFolder_Choose;
#endif

        public string DefaultFolder => AppPaths.DefaultFolder();

        public string SelectedFolder => AppPaths.SelectedFolder();

#if WINDOWS || ANDROID
        public bool IsCloudFileSupported => true;
#else
        public bool IsCloudFileSupported => false;
#endif

        public string? CloudFileName => AppPaths.CloudFile()?.Name;

        public async Task<bool> PickCloudFileAsync()
        {
#if ANDROID
            var uri = await CloudFilePicker.PickAsync();
            if (uri is null) return false;

            var document = new AndroidScheduleDocument(uri, AndroidScheduleDocument.DescribeWritable(uri));
            var text = ReadSchedule(document);
            CloudFilePicker.KeepAccess(uri);
            Link(uri.ToString()!, document, text);
            return true;
#elif WINDOWS
            var path = await PickFilePathAsync();
            if (path is null) return false;
            if (File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly))
                throw new UnauthorizedAccessException(Strings.Error_CloudFileReadOnly);

            var document = new FileScheduleDocument(path);
            Link(path, document, ReadSchedule(document));
            return true;
#else
            await Task.CompletedTask;
            return false;
#endif
        }

#if WINDOWS || ANDROID
        // Rifiuta un file scelto per sbaglio, prima che un salvataggio lo sovrascriva
        private static string ReadSchedule(IScheduleDocument document)
        {
            var text = document.ReadText();
            return LinkedFileEventRepository.IsScheduleText(text)
                ? text
                : throw new IOException(Strings.Error_NotAScheduleFile);
        }

        // Come per le cartelle: il file in uso segue l'utente, senza sovrascrivere uno che ha già un contenuto
        private static void Link(string address, IScheduleDocument document, string text)
        {
            var current = AppPaths.CurrentSchedulePath();
            if (string.IsNullOrWhiteSpace(text) && File.Exists(current))
            {
                text = File.ReadAllText(current);
                document.WriteText(text);
            }

            AppPaths.CloudCache(address).Mirror(text);
            AppPaths.StoreCloudFile(address, document.Name);
        }
#endif

#if WINDOWS
        // Null se l'utente annulla
        private static async Task<string?> PickFilePathAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add("*");
            if (!TryAttachToWindow(picker)) return null;

            var file = await picker.PickSingleFileAsync();
            if (file is null) return null;

            // Un file senza percorso non si può rileggere al prossimo avvio
            return string.IsNullOrEmpty(file.Path) ? throw new IOException(file.Name) : file.Path;
        }

        // Un programma non pacchettizzato deve dire alle finestre di scelta a quale finestra appartengono
        private static bool TryAttachToWindow(object picker)
        {
            var window = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (window is null) return false;

            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));
            return true;
        }
#endif

        public async Task<string?> PickFolderAsync()
        {
#if WINDOWS
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");

            if (!TryAttachToWindow(picker)) return null;

            var folder = await picker.PickSingleFolderAsync();
            return folder?.Path;
#elif ANDROID
            if (!await StorageAccess.RequestAccessAsync())
                throw new UnauthorizedAccessException(Strings.Error_StoragePermissionDenied);
            return StorageAccess.ScheduleFolder;
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
            var source = AppPaths.CurrentSchedulePath();
            var destination = Path.Combine(target, AppPaths.ScheduleFileName);
            if (File.Exists(source) && !File.Exists(destination))
                File.Copy(source, destination);

            AppPaths.ForgetCloudFile();
            AppPaths.StoreSelectedFolder(string.IsNullOrWhiteSpace(folder) ? null : folder);
        }
    }
}
