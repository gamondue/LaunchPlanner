using System.Text;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Scheduling;

namespace LaunchPlanner.Core.Persistence;

public sealed class FileEventRepository(string schedulePath, string firedPath) : IEventRepository
{
    private static readonly Encoding Utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    public string SchedulePath => schedulePath;

    public ScheduleParseResult Load()
        => File.Exists(schedulePath)
            ? ScheduleFile.Parse(File.ReadAllText(schedulePath, Encoding.UTF8))
            : new ScheduleParseResult([], []);

    // Il salvataggio riscrive solo gli eventi validi: commenti e righe errate restano nella copia .bak
    public void Save(IReadOnlyList<ScheduledEvent> events) => Replace(ScheduleFile.Format(events));

    // Allinea il file a un testo preso altrove. Se non cambia nulla non scrive,
    // altrimenti la copia .bak perderebbe la versione precedente
    public void Mirror(string text)
    {
        if (!File.Exists(schedulePath) || File.ReadAllText(schedulePath, Encoding.UTF8) != text)
            Replace(text);
    }

    private void Replace(string text)
    {
        if (File.Exists(schedulePath))
            File.Copy(schedulePath, schedulePath + ".bak", overwrite: true);
        WriteAtomically(schedulePath, text);
    }

    public FiredLog LoadFired()
        => File.Exists(firedPath) ? FiredLog.Deserialize(File.ReadAllText(firedPath)) : new FiredLog();

    public void SaveFired(FiredLog fired) => WriteAtomically(firedPath, fired.Serialize());

    // Un processo interrotto a metà scrittura non deve rovinare il file
    private static void WriteAtomically(string path, string content)
    {
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, content, Utf8WithBom);
        File.Move(temporary, path, overwrite: true);
    }
}
