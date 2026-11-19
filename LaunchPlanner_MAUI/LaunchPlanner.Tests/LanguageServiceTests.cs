using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;
using LaunchPlanner.Core.ViewModels;

namespace LaunchPlanner.Tests;

public sealed class LanguageServiceTests : IDisposable
{
    private sealed class FakeStore : ILanguageStore
    {
        public string Language { get; set; } = LanguageService.SystemLanguage;
    }

    private readonly FakeStore store = new();
    private readonly LanguageService language;

    public LanguageServiceTests() => language = new LanguageService(store);

    public void Dispose() => Strings.Culture = null;

    [Fact]
    public void Apply_UsesTheSavedLanguage()
    {
        store.Language = "en";

        language.Apply();

        Assert.Equal("en", language.Current);
        Assert.Equal("Yes", Strings.Common_Yes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("xx")]
    [InlineData("not a language")]
    public void Apply_FollowsTheSystem_WithoutAValidChoice(string saved)
    {
        store.Language = saved;

        language.Apply();

        Assert.Null(Strings.Culture);
        Assert.Equal(LanguageService.SystemLanguage, language.Current);
        Assert.Equal("Sì", Strings.Common_Yes); // i test girano in italiano
    }

    [Fact]
    public void Change_SavesAppliesAndNotifies()
    {
        var notifications = 0;
        language.Changed += () => notifications++;

        language.Change("en");

        Assert.Equal("en", store.Language);
        Assert.Equal("Yes", Strings.Common_Yes);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public void Change_DoesNothing_WhenTheLanguageIsAlreadyInUse()
    {
        language.Change("en");
        var notifications = 0;
        language.Changed += () => notifications++;

        language.Change("en");

        Assert.Equal(0, notifications);
    }

    [Fact]
    public void Change_BackToSystem_ForgetsTheChoice()
    {
        language.Change("en");

        language.Change(LanguageService.SystemLanguage);

        Assert.Equal(LanguageService.SystemLanguage, store.Language);
        Assert.Null(Strings.Culture);
    }

    [Fact]
    public void Options_OfferSystemItalianAndEnglish()
    {
        Assert.Equal(
            [LanguageService.SystemLanguage, "it", "en"],
            language.Options.Select(option => option.Code));
    }

    [Fact]
    public void Options_ShowEachLanguageInItsOwnName_WhateverTheCurrentLanguage()
    {
        language.Change("en");

        Assert.Equal(
            ["System language", "Italiano", "English"],
            language.Options.Select(option => option.Caption));
    }

    [Fact]
    public void PeriodCaptions_FollowTheChange_AndKeepTheSelection()
    {
        var monday = PeriodOption.All.Single(option => option.Period == Periodicity.Monday);
        Assert.Equal("lunedì", monday.Caption);

        language.Change("en");

        Assert.Equal("Monday", monday.Caption);
        Assert.Same(monday, PeriodOption.All.Single(option => option.Period == Periodicity.Monday));
    }

    [Fact]
    public void EventRow_ShowsTheNewLanguageWithoutBeingRecreated()
    {
        var row = new EventRow(new ScheduledEvent { Period = Periodicity.Day, Enabled = true });

        language.Change("en");

        Assert.Equal("day", row.Period);
        Assert.Equal("Yes", row.Enabled);
    }

    [Fact]
    public void AboutViewModel_ChangesLanguageFromThePicker()
    {
        var about = new AboutViewModel("1.0", language);
        Assert.Equal(LanguageService.SystemLanguage, about.SelectedLanguage?.Code);

        about.SelectedLanguage = about.Languages.Single(option => option.Code == "en");

        Assert.Equal("en", store.Language);
        Assert.Equal("en", about.SelectedLanguage?.Code);
        Assert.Equal("Version 1.0", about.Version);
    }

    [Fact]
    public void AboutViewModel_IgnoresThePickerBeingCleared()
    {
        language.Change("en");
        var about = new AboutViewModel("1.0", language);

        about.SelectedLanguage = null;

        Assert.Equal("en", store.Language);
    }
}
