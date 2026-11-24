using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;
using LaunchPlanner.Core.ViewModels;

namespace LaunchPlanner.Tests;

public sealed class LanguageServiceTests : IDisposable
{
    private sealed class FakeStore : ILanguageStore
    {
        public string Language { get; set; } = LanguageService.English;
    }

    private sealed class FakeStartup : IStartupSettings
    {
        public bool IsSupported => false;
        public string Caption => "";
        public bool IsEnabled { get; set; }
    }

    private sealed class FakeDataFolder : IDataFolderSettings
    {
        public bool IsSupported => false;
        public string DefaultFolder => "";
        public string SelectedFolder => "";
        public Task<string?> PickFolderAsync() => Task.FromResult<string?>(null);
        public void Select(string? folder) { }
    }

    private readonly FakeStore store = new();
    private readonly LanguageService language;

    public LanguageServiceTests()
    {
        language = new LanguageService(store);
        language.Apply();
    }

    public void Dispose() => Strings.Culture = null;

    private SettingsViewModel NewSettings() => new(new FakeStartup(), new FakeDataFolder(), language, new ActivityLog());

    [Theory]
    [InlineData("en", "Yes")]
    [InlineData("it", "Sì")]
    public void Apply_UsesTheSavedLanguage(string saved, string expectedYes)
    {
        store.Language = saved;

        language.Apply();

        Assert.Equal(saved, language.Current);
        Assert.Equal(expectedYes, Strings.Common_Yes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("xx")]
    [InlineData("not a language")]
    public void Apply_FallsBackToEnglish_WithoutAValidChoice(string saved)
    {
        store.Language = saved;

        language.Apply();

        Assert.Equal(LanguageService.English, language.Current);
        Assert.Equal("Yes", Strings.Common_Yes);
    }

    [Fact]
    public void Change_SavesAppliesAndNotifies()
    {
        var notifications = 0;
        language.Changed += () => notifications++;

        language.Change("it");

        Assert.Equal("it", store.Language);
        Assert.Equal("Sì", Strings.Common_Yes);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public void Change_DoesNothing_WhenTheLanguageIsAlreadyInUse()
    {
        language.Change("it");
        var notifications = 0;
        language.Changed += () => notifications++;

        language.Change("it");

        Assert.Equal(0, notifications);
    }

    [Fact]
    public void Change_BackToEnglish_SavesTheChoice()
    {
        language.Change("it");

        language.Change(LanguageService.English);

        Assert.Equal(LanguageService.English, store.Language);
        Assert.Equal("Yes", Strings.Common_Yes);
    }

    [Fact]
    public void Options_OfferEnglishAndItalian_EnglishFirst()
    {
        Assert.Equal(["en", "it"], language.Options.Select(option => option.Code));
    }

    [Fact]
    public void Options_ShowEachLanguageInItsOwnName_WhateverTheCurrentLanguage()
    {
        language.Change("it");

        Assert.Equal(["English", "Italiano"], language.Options.Select(option => option.Caption));
    }

    [Fact]
    public void PeriodCaptions_FollowTheChange_AndKeepTheSelection()
    {
        var monday = PeriodOption.All.Single(option => option.Period == Periodicity.Monday);
        language.Change("it");
        Assert.Equal("lunedì", monday.Caption);

        language.Change("en");

        Assert.Equal("Monday", monday.Caption);
        Assert.Same(monday, PeriodOption.All.Single(option => option.Period == Periodicity.Monday));
    }

    [Fact]
    public void OffsetUnitCaptions_FollowTheChange()
    {
        var months = NowOffsetUnit.Default;
        Assert.Equal("months", months.Caption);

        language.Change("it");

        Assert.Equal("mesi", months.Caption);
    }

    [Fact]
    public void EventRow_ShowsTheNewLanguageWithoutBeingRecreated()
    {
        var row = new EventRow(new ScheduledEvent { Period = Periodicity.Day, Enabled = true });
        Assert.Equal("Yes", row.Enabled);

        language.Change("it");

        Assert.Equal("Sì", row.Enabled);
    }

    [Fact]
    public void SettingsViewModel_StartsInEnglish_OnFirstLaunch()
    {
        var settings = NewSettings();

        Assert.Equal("en", settings.SelectedLanguage?.Code);
    }

    [Fact]
    public void SettingsViewModel_ChangesLanguageFromThePicker()
    {
        var settings = NewSettings();

        settings.SelectedLanguage = settings.Languages.Single(option => option.Code == "it");

        Assert.Equal("it", store.Language);
        Assert.Equal("it", settings.SelectedLanguage?.Code);
        Assert.Equal("Sì", Strings.Common_Yes);
    }

    [Fact]
    public void SettingsViewModel_IgnoresThePickerBeingCleared()
    {
        language.Change("it");
        var settings = NewSettings();

        settings.SelectedLanguage = null;

        Assert.Equal("it", store.Language);
    }

    [Fact]
    public void AboutViewModel_TranslatesVersionAfterAChange()
    {
        var about = new AboutViewModel("1.0");
        Assert.Equal("Version 1.0", about.Version);

        language.Change("it");

        Assert.Equal("Versione 1.0", about.Version);
    }
}
