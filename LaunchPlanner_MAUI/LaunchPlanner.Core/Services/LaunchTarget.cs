namespace LaunchPlanner.Core.Services;

public static class LaunchTarget
{
    // Se è un URI file:// restituisce il percorso locale
    public static string? Resolve(string? url)
    {
        var target = url?.Trim();
        if (string.IsNullOrEmpty(target))
            return null;

        return Uri.TryCreate(target, UriKind.Absolute, out var uri) && uri.IsFile ? uri.LocalPath : target;
    }
}
