using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;
using LaunchPlanner.Core.ViewModels;

namespace LaunchPlanner.Tests;

public class LocalizationTests
{
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it");
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr");

    private sealed class NoLanguageStore : ILanguageStore
    {
        public string Language { get; set; } = LanguageService.SystemLanguage;
    }

    private static Dictionary<string, string> StringsOf(CultureInfo culture)
        => Strings.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false)!
            .Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);

    private static void WithCulture(string name, Action action)
    {
        var culture = CultureInfo.GetCultureInfo(name);
        var (previous, previousUi) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        try
        {
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    private static IEnumerable<string> Placeholders(string text)
        => Regex.Matches(text, @"\{\d+\}").Select(match => match.Value).Order();

    [Fact]
    public void Italian_HasExactlyTheEnglishKeys()
    {
        var english = StringsOf(CultureInfo.InvariantCulture);
        var italian = StringsOf(Italian);

        Assert.NotEmpty(english);
        Assert.Equal(english.Keys.Order(), italian.Keys.Order());
    }

    [Fact]
    public void Italian_KeepsThePlaceholdersOfEveryEnglishString()
    {
        var italian = StringsOf(Italian);

        foreach (var (key, english) in StringsOf(CultureInfo.InvariantCulture))
            Assert.True(Placeholders(english).SequenceEqual(Placeholders(italian[key])), key);
    }

    [Fact]
    public void EveryString_HasText()
    {
        Assert.All(StringsOf(CultureInfo.InvariantCulture).Values, text => Assert.False(string.IsNullOrWhiteSpace(text)));
        Assert.All(StringsOf(Italian).Values, text => Assert.False(string.IsNullOrWhiteSpace(text)));
    }

    [Theory]
    [InlineData("it", "Sì")]
    [InlineData("it-IT", "Sì")]
    [InlineData("en", "Yes")]
    [InlineData("en-GB", "Yes")]
    [InlineData("fr", "Yes")] // le lingue senza traduzione ricadono sull'inglese
    public void Strings_FollowTheSystemLanguage(string culture, string expected)
    {
        Assert.Equal(expected, Strings.ResourceManager.GetString(nameof(Strings.Common_Yes), CultureInfo.GetCultureInfo(culture)));
    }

    [Fact]
    public void EveryPeriodicity_HasACaptionInBothLanguages()
    {
        var english = StringsOf(CultureInfo.InvariantCulture);
        var italian = StringsOf(Italian);

        foreach (var period in Enum.GetValues<Periodicity>())
        {
            Assert.Contains(PeriodOption.All, option => option.Period == period);
            Assert.Contains($"Period_{period}", english.Keys);
            Assert.Contains($"Period_{period}", italian.Keys);
        }
    }

    [Fact]
    public void ParseErrors_FollowTheUserInterfaceLanguage()
    {
        WithCulture("en-US", () =>
        {
            var result = ScheduleFile.Parse("header\nshort line\n");

            Assert.Equal("Too few fields (line 2)", Assert.Single(result.Errors));
        });
    }

    [Fact]
    public void ScheduleFile_DoesNotDependOnTheUserInterfaceLanguage()
    {
        ScheduledEvent ev = new()
        {
            Start = new DateTime(2018, 10, 2, 14, 50, 0),
            Finish = new DateTime(2030, 6, 10, 15, 5, 0),
            Duration = TimeSpan.FromMinutes(15),
            Period = Periodicity.Monday,
            Description = "d",
            Url = "u",
            Enabled = true,
        };
        var italian = ScheduleFile.Format([ev]);

        WithCulture("en-US", () =>
        {
            Assert.Equal(italian, ScheduleFile.Format([ev]));
            Assert.Equal(Periodicity.Monday, Assert.Single(ScheduleFile.Parse(italian).Events).Period);
        });
        Assert.StartsWith("Istante Inizio\t", italian);
    }

    [Theory]
    [InlineData("it-IT", "02/10/2018 14:50:00", "Sì")]
    [InlineData("en-GB", "02/10/2018 14:50:00", "Yes")]
    public void EventRow_FollowsTheUserCulture(string culture, string expectedStart, string expectedEnabled)
    {
        WithCulture(culture, () =>
        {
            var row = new EventRow(new ScheduledEvent { Start = new DateTime(2018, 10, 2, 14, 50, 0), Enabled = true });

            Assert.Equal(expectedStart, row.Start);
            Assert.Equal(expectedEnabled, row.Enabled);
        });
    }

    [Fact]
    public void EventRow_UsesTheAmericanDateOrderInTheUnitedStates()
    {
        WithCulture("en-US", () =>
        {
            var row = new EventRow(new ScheduledEvent { Start = new DateTime(2018, 10, 2, 14, 50, 0) });

            Assert.StartsWith("10/2/2018", row.Start);
        });
    }

    [Fact]
    public void AboutPage_TranslatesVersionAndCountry()
    {
        WithCulture("en-US", () =>
        {
            var about = new AboutViewModel("1.0", new LanguageService(new NoLanguageStore()));

            Assert.Equal("Version 1.0", about.Version);
            Assert.Equal("ITALY", about.Country);
        });
    }

    [Fact]
    public void FrenchSystem_GetsTheEnglishDisclaimer()
    {
        Assert.Equal(
            Strings.ResourceManager.GetString(nameof(Strings.About_Disclaimer), CultureInfo.InvariantCulture),
            Strings.ResourceManager.GetString(nameof(Strings.About_Disclaimer), French));
    }
}
