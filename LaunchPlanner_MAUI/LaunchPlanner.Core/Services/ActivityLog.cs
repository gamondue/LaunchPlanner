using CommunityToolkit.Mvvm.ComponentModel;

namespace LaunchPlanner.Core.Services;

// Registro dei lanci e titolo corrente, mostrati dalla pagina principale e dal tray
public sealed partial class ActivityLog : ObservableObject
{
    private const int MaximumLength = 100_000;
    public const string DefaultStatus = "LaunchPlanner";

    [ObservableProperty]
    public partial string Text { get; private set; } = "";

    [ObservableProperty]
    public partial string Status { get; set; } = DefaultStatus;

    public event Action<string>? ErrorReported;

    public void Add(string message)
    {
        var text = Text + $"{DateTime.Now} {message}\r\n";
        Text = text.Length > MaximumLength ? text[^MaximumLength..] : text;
    }

    public void AddError(string message)
    {
        Add(message);
        ErrorReported?.Invoke(message);
    }
}
