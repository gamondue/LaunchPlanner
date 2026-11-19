namespace LaunchPlanner.Core.Models;

public sealed record LanguageOption(string Code, string Caption)
{
    public override string ToString() => Caption;
}
