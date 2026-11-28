using System.Text;
using Android.Content;
using Android.Provider;
using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Services
{
    // Il file nel cloud visto attraverso il suo URI: lo legge e scrive l'app del servizio (OneDrive, Drive...),
    // che può dover andare in rete. Le eccezioni Java diventano quelle che il resto del programma gestisce
    public sealed class AndroidScheduleDocument(Android.Net.Uri uri, string name) : IScheduleDocument
    {
        // Sotto i 10 secondi concessi a un BroadcastReceiver, quello della sveglia
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(8);

        // Un file di eventi è piccolo: oltre questa lunghezza è stato scelto un altro file
        private const int MaxLength = 1024 * 1024;

        private static ContentResolver Resolver => Android.App.Application.Context.ContentResolver!;

        public string Name => name;

        public string ReadText() => Run(() =>
        {
            using var stream = Resolver.OpenInputStream(uri) ?? throw new IOException(name);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var buffer = new char[MaxLength + 1];
            var length = reader.ReadBlock(buffer, 0, buffer.Length);
            return length > MaxLength
                ? throw new IOException(Strings.Error_NotAScheduleFile)
                : new string(buffer, 0, length);
        });

        // "wt" tronca il file prima di scrivere, altrimenti resterebbe la coda della versione più lunga
        public void WriteText(string text) => Run(() =>
        {
            using var stream = Resolver.OpenOutputStream(uri, "wt") ?? throw new IOException(name);
            using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            writer.Write(text);
            return true;
        });

        // Il nome da mostrare all'utente; lancia UnauthorizedAccessException se il file non si può scrivere
        public static string DescribeWritable(Android.Net.Uri uri) => Run(() =>
        {
            string[] columns = [DocumentsContract.Document.ColumnDisplayName, DocumentsContract.Document.ColumnFlags];
            using var cursor = Resolver.Query(uri, columns, null, null, null);
            if (cursor is null || !cursor.MoveToFirst())
                throw new IOException(uri.ToString());

            var flags = (DocumentContractFlags)cursor.GetInt(1);
            if (!flags.HasFlag(DocumentContractFlags.SupportsWrite))
                throw new UnauthorizedAccessException(Strings.Error_CloudFileReadOnly);

            return cursor.GetString(0) ?? uri.LastPathSegment ?? uri.ToString()!;
        });

        // Su un altro thread, per non bloccare per sempre chi chiama se il servizio non risponde
        private static T Run<T>(Func<T> action)
        {
            var task = Task.Run(action);
            try
            {
                return task.Wait(Timeout) ? task.Result : throw new IOException(Strings.Error_CloudTimeout);
            }
            catch (System.AggregateException ex)
            {
                var cause = ex.GetBaseException();
                if (cause is UnauthorizedAccessException or Java.Lang.SecurityException)
                    throw new UnauthorizedAccessException(cause.Message, cause);
                throw new IOException(cause.Message, cause);
            }
        }
    }
}
