using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Tests;

public sealed class LinkedFileEventRepositoryTests : IDisposable
{
    private sealed class FakeDocument : IScheduleDocument
    {
        public string Name => "cloud.txt";
        public string Text { get; set; } = "";
        public bool IsOffline { get; set; }
        public string ReadText() => IsOffline ? throw new IOException("senza rete") : Text;

        public void WriteText(string text)
        {
            if (IsOffline) throw new IOException("senza rete");
            Text = text;
        }
    }

    private static readonly ScheduledEvent Sample = new()
    {
        Start = new DateTime(2026, 1, 1, 8, 0, 0),
        Finish = new DateTime(2026, 12, 31, 8, 0, 0),
        Description = "Esempio",
        Url = "https://example.com",
    };

    private readonly string folder = Directory.CreateTempSubdirectory("lp-linked").FullName;
    private readonly FakeDocument document = new();
    private readonly ActivityLog log = new();
    private readonly FileEventRepository cache;
    private readonly LinkedFileEventRepository repository;

    public LinkedFileEventRepositoryTests()
    {
        cache = new FileEventRepository(Path.Combine(folder, "cache.txt"), Path.Combine(folder, "fired.json"));
        repository = new LinkedFileEventRepository(cache, document, log);
    }

    public void Dispose() => Directory.Delete(folder, recursive: true);

    [Fact]
    public void Load_ReadsDocumentAndKeepsLocalCopy()
    {
        document.Text = ScheduleFile.Format([Sample]);

        var result = repository.Load();

        Assert.Equal("Esempio", Assert.Single(result.Events).Description);
        Assert.Single(cache.Load().Events);
    }

    [Fact]
    public void Load_UnchangedDocumentDoesNotOverwriteBackup()
    {
        document.Text = ScheduleFile.Format([Sample]);
        repository.Load();
        document.Text = ScheduleFile.Format([]);
        repository.Load();
        repository.Load();

        Assert.Single(ScheduleFile.Parse(File.ReadAllText(cache.SchedulePath + ".bak")).Events);
    }

    [Fact]
    public void Load_OfflineUsesLocalCopyAndReportsIt()
    {
        document.Text = ScheduleFile.Format([Sample]);
        repository.Load();
        document.IsOffline = true;

        var result = repository.Load();

        Assert.Single(result.Events);
        Assert.Contains("senza rete", log.Text);
    }

    [Fact]
    public void Load_OfflineWithoutLocalCopyFails()
    {
        document.IsOffline = true;

        Assert.Throws<IOException>(() => repository.Load());
    }

    [Fact]
    public void Save_WritesDocumentAndLocalCopy()
    {
        repository.Save([Sample]);

        Assert.Single(ScheduleFile.Parse(document.Text).Events);
        Assert.Single(cache.Load().Events);
    }

    [Fact]
    public void Save_OfflineFailsAndLeavesLocalCopyUntouched()
    {
        repository.Save([]);
        document.IsOffline = true;

        Assert.Throws<IOException>(() => repository.Save([Sample]));
        Assert.Empty(cache.Load().Events);
    }

    [Fact]
    public void FileDocument_ReplacesAndReadsTheFile()
    {
        var path = Path.Combine(folder, "drive.txt");
        File.WriteAllText(path, "una versione precedente, più lunga");
        var file = new FileScheduleDocument(path);

        file.WriteText("nuova");

        Assert.Equal("nuova", file.ReadText());
        Assert.Equal(path, file.Name);
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void FileDocument_UnavailableFileFallsBackToLocalCopy()
    {
        cache.Save([Sample]);
        var missing = new FileScheduleDocument(Path.Combine(folder, "non-montato", "drive.txt"));

        var result = new LinkedFileEventRepository(cache, missing, log).Load();

        Assert.Single(result.Events);
        Assert.NotEmpty(log.Text);
    }

    [Fact]
    public void FileDocument_RejectsOversizedFile()
    {
        var path = Path.Combine(folder, "grande.txt");
        File.WriteAllText(path, new string('a', 2 * 1024 * 1024));

        Assert.Throws<IOException>(() => new FileScheduleDocument(path).ReadText());
    }

    [Fact]
    public void SchedulePath_IsTheDocumentName() => Assert.Equal("cloud.txt", repository.SchedulePath);

    [Theory]
    [InlineData("", true)]
    [InlineData("Istante Inizio\tIstante Fine\r\n", true)]
    [InlineData("un testo qualsiasi", false)]
    [InlineData("intestazione\nriga che non è un evento", false)]
    public void IsScheduleText_RecognizesScheduleFiles(string text, bool expected)
        => Assert.Equal(expected, LinkedFileEventRepository.IsScheduleText(text));

    [Fact]
    public void IsScheduleText_AcceptsFileWithEvents()
        => Assert.True(LinkedFileEventRepository.IsScheduleText(ScheduleFile.Format([Sample])));
}
