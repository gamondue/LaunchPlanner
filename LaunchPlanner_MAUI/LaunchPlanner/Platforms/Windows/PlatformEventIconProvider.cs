using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Services
{
    // Trova il programma che Windows userà per l'evento e ne restituisce l'icona in PNG
    public sealed class PlatformEventIconProvider : IEventIconProvider
    {
        private const string ExecutableExtension = ".exe";
        private const string OpenVerb = "open";
        private const int MaximumPathLength = 1024;
        private const int S_OK = 0;
        private const uint ASSOCF_NOTRUNCATE = 0x20;
        private const uint ASSOCSTR_EXECUTABLE = 2;

        // Programma associato per estensione o schema, e icona per file: il registro e il disco
        // si leggono una volta sola, perché la riga chiede l'icona a ogni aggiornamento
        private readonly ConcurrentDictionary<string, string?> associations = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, byte[]?> iconsByFile = new(StringComparer.OrdinalIgnoreCase);

        public byte[]? GetIconPng(string? url)
        {
            var file = IconFileFor(url);
            return file is null ? null : iconsByFile.GetOrAdd(file, ExtractPng);
        }

        // L'eseguibile stesso, altrimenti il programma associato all'estensione o allo schema dell'URL.
        // Se nessun programma è registrato (es. collegamenti .lnk) si usa l'icona del file, se esiste.
        private string? IconFileFor(string? url)
        {
            var target = LaunchTarget.Resolve(url);
            if (target is null)
                return null;

            if (Uri.TryCreate(target, UriKind.Absolute, out var uri) && !uri.IsFile)
                return associations.GetOrAdd(uri.Scheme, AssociatedExecutable);

            var extension = Path.GetExtension(target);
            if (string.IsNullOrEmpty(extension))
                return null;
            if (extension.Equals(ExecutableExtension, StringComparison.OrdinalIgnoreCase))
                return File.Exists(target) ? target : null;

            return associations.GetOrAdd(extension, AssociatedExecutable)
                ?? (File.Exists(target) ? target : null);
        }

        private static string? AssociatedExecutable(string extensionOrScheme)
        {
            var buffer = new StringBuilder(MaximumPathLength);
            var length = (uint)buffer.Capacity;
            var result = AssocQueryString(
                ASSOCF_NOTRUNCATE, ASSOCSTR_EXECUTABLE, extensionOrScheme, OpenVerb, buffer, ref length);
            if (result != S_OK)
                return null;

            var executable = buffer.ToString();
            return File.Exists(executable) ? executable : null;
        }

        private static byte[]? ExtractPng(string file)
        {
            try
            {
                using var icon = System.Drawing.Icon.ExtractAssociatedIcon(file);
                if (icon is null)
                    return null;

                using var bitmap = icon.ToBitmap();
                using var stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
            catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException or ExternalException)
            {
                // Un'icona mancante non deve impedire di mostrare la riga
                return null;
            }
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int AssocQueryString(
            uint flags, uint str, string pszAssoc, string? pszExtra, StringBuilder? pszOut, ref uint pcchOut);
    }
}
