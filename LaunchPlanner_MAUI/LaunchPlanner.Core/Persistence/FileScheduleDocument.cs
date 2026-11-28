using System.Text;
using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.Persistence;

// Un file scelto dall'utente dove il cloud è una normale cartella, come su Windows. Il nome è il percorso intero.
// Se non è raggiungibile (per esempio Google Drive non ancora avviato) lancia IOException
public sealed class FileScheduleDocument(string path) : IScheduleDocument
{
    // Un file di eventi è piccolo: oltre questa dimensione è stato scelto un altro file
    private const long MaxBytes = 1024 * 1024;

    public string Name => path;

    public string ReadText()
        => new FileInfo(path).Length > MaxBytes
            ? throw new IOException(Strings.Error_NotAScheduleFile)
            : File.ReadAllText(path, Encoding.UTF8);

    // Un processo interrotto a metà scrittura non deve rovinare il file
    public void WriteText(string text)
    {
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        File.Move(temporary, path, overwrite: true);
    }
}
