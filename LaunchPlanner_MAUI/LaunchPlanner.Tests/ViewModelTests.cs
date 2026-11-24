using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Persistence;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Scheduling;
using LaunchPlanner.Core.Services;
using LaunchPlanner.Core.ViewModels;

namespace LaunchPlanner.Tests;

public sealed class ViewModelTests
{
    private static readonly DateTime Now = new(2026, 3, 9, 14, 50, 0);

    private sealed class FakeRepository : IEventRepository
    {
        public string Text { get; set; } = "";
        public FiredLog Fired { get; private set; } = new();
        public int Saves { get; private set; }
        public bool IsScheduleLocked { get; set; }
        public bool IsFiredLocked { get; set; }
        public string SchedulePath => "schedule.txt";
        public ScheduleParseResult Load()
            => IsScheduleLocked ? throw new IOException("bloccato") : ScheduleFile.Parse(Text);
        public Exception? SaveFailure { get; set; }
        public void Save(IReadOnlyList<ScheduledEvent> events)
        {
            if (SaveFailure is not null) throw SaveFailure;
            Text = ScheduleFile.Format(events);
            Saves++;
        }
        public FiredLog LoadFired() => IsFiredLocked ? throw new IOException("bloccato") : Fired;
        public void SaveFired(FiredLog fired) => Fired = fired;
    }

    private sealed class FakeLauncher : IEventLauncher
    {
        public List<string> Opened { get; } = [];
        public string? Failure { get; set; }

        public Task LaunchAsync(ScheduledEvent ev)
        {
            if (Failure is not null) throw new InvalidOperationException(Failure);
            Opened.Add(ev.Url);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBackground : IBackgroundScheduler
    {
        public int Rearms { get; private set; }
        public void Start() { }
        public void Rearm() => Rearms++;
    }

    private sealed class FakeStartup : IStartupSettings
    {
        public bool IsSupported { get; init; } = true;
        public string Caption => "avvio";
        public bool IsEnabled { get; set; }
    }

    private sealed class FakeDataFolder : IDataFolderSettings
    {
        public bool IsSupported { get; init; } = true;
        public string DefaultFolder => Path.Combine(Path.GetTempPath(), "lp-default");
        public string SelectedFolder { get; private set; } = Path.Combine(Path.GetTempPath(), "lp-default");
        public string? Picked { get; set; }
        public bool FailOnSelect { get; set; }

        public Task<string?> PickFolderAsync() => Task.FromResult(Picked);

        public void Select(string? folder)
        {
            if (FailOnSelect) throw new IOException("sola lettura");
            SelectedFolder = folder ?? DefaultFolder;
        }
    }

    private sealed class FakeLanguageStore : ILanguageStore
    {
        public string Language { get; set; } = LanguageService.English;
    }

    private static SettingsViewModel NewSettings(
        IStartupSettings? startup = null, IDataFolderSettings? dataFolder = null, ActivityLog? log = null)
        => new(
            startup ?? new FakeStartup(),
            dataFolder ?? new FakeDataFolder(),
            new LanguageService(new FakeLanguageStore()),
            log ?? new ActivityLog());

    [Fact]
    public async Task Settings_ChoosingFolderStoresItAndAsksForRestart()
    {
        var folder = new FakeDataFolder { Picked = Path.Combine(Path.GetTempPath(), "lp-custom") };
        var settings = NewSettings(dataFolder: folder);
        Assert.False(settings.IsCustomDataFolder);

        await settings.ChooseDataFolderCommand.ExecuteAsync(null);

        Assert.Equal(folder.Picked, folder.SelectedFolder);
        Assert.Equal(folder.Picked, settings.DataFolder);
        Assert.True(settings.IsCustomDataFolder);
        Assert.True(settings.IsRestartNeeded);
    }

    [Fact]
    public async Task Settings_CancelledPickerChangesNothing()
    {
        var folder = new FakeDataFolder { Picked = null };
        var settings = NewSettings(dataFolder: folder);

        await settings.ChooseDataFolderCommand.ExecuteAsync(null);

        Assert.False(settings.IsRestartNeeded);
        Assert.False(settings.IsCustomDataFolder);
    }

    [Fact]
    public async Task Settings_UseDefaultRestoresDefaultFolder()
    {
        var folder = new FakeDataFolder { Picked = Path.Combine(Path.GetTempPath(), "lp-custom") };
        var settings = NewSettings(dataFolder: folder);
        await settings.ChooseDataFolderCommand.ExecuteAsync(null);

        settings.UseDefaultDataFolderCommand.Execute(null);

        Assert.Equal(folder.DefaultFolder, settings.DataFolder);
        Assert.False(settings.IsCustomDataFolder);
    }

    [Fact]
    public async Task Settings_FolderChangeFailureIsLoggedAndNotApplied()
    {
        var log = new ActivityLog();
        var folder = new FakeDataFolder { Picked = Path.Combine(Path.GetTempPath(), "lp-custom"), FailOnSelect = true };
        var settings = NewSettings(dataFolder: folder, log: log);

        await settings.ChooseDataFolderCommand.ExecuteAsync(null);

        Assert.False(settings.IsRestartNeeded);
        Assert.False(settings.IsCustomDataFolder);
        Assert.NotEmpty(log.Text);
    }

    [Fact]
    public void Settings_WritesStartupChoiceThroughToPlatform()
    {
        var startup = new FakeStartup();
        var settings = NewSettings(startup);

        settings.IsStartupEnabled = true;

        Assert.True(startup.IsEnabled);
        Assert.True(settings.IsStartupSupported);
        Assert.Equal("avvio", settings.StartupCaption);
    }

    [Fact]
    public void Settings_HidesPlatformOptionsThatAreNotSupported()
    {
        var settings = NewSettings(new FakeStartup { IsSupported = false }, new FakeDataFolder { IsSupported = false });

        Assert.False(settings.IsStartupSupported);
        Assert.False(settings.IsDataFolderSupported);
        Assert.Equal(["en", "it"], settings.Languages.Select(option => option.Code));
    }

    private sealed class FakeShell : IShellActions
    {
        public int Closes { get; private set; }
        public int FileEdits { get; private set; }
        public bool CanEditFile => true;
        public bool CanExit => true;
        public void CloseEditor() => Closes++;
        public void OpenScheduleFile() => FileEdits++;
        public void Exit() { }
    }

    // L'"icona" è l'URL stesso in byte: basta a riconoscere per quale evento è stata chiesta
    private sealed class FakeIcons : IEventIconProvider
    {
        public byte[]? GetIconPng(string? url)
            => string.IsNullOrEmpty(url) ? null : System.Text.Encoding.UTF8.GetBytes(url);
    }

    private sealed class Fixture
    {
        public FakeRepository Repository { get; } = new();
        public FakeLauncher Launcher { get; } = new();
        public FakeBackground Background { get; } = new();
        public FakeShell Shell { get; } = new();
        public ActivityLog Log { get; } = new();
        public FakeIcons Icons { get; } = new();
        public EventStore Store { get; }
        public FiringService Firing { get; }
        public EventsViewModel Events { get; }

        public Fixture()
        {
            Store = new EventStore(Repository, Log);
            Firing = new FiringService(Store, Repository, Launcher, Log);
            Events = new EventsViewModel(Store, Launcher, Background, Shell, Log, Icons);
        }
    }

    private static ScheduledEvent Sample(string description = "e") => new()
    {
        Start = Now,
        Finish = Now.AddYears(1),
        Duration = TimeSpan.FromMinutes(15),
        Period = Periodicity.Day,
        Description = description,
        Url = "https://example.org",
        Enabled = true,
    };

    [Fact]
    public void UnsavedChanges_AreOffAfterLoading()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);

        Assert.False(f.Events.HasUnsavedChanges);
    }

    [Fact]
    public void UnsavedChanges_FollowEditsAndSave()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        var notified = 0;
        f.Events.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(EventsViewModel.HasUnsavedChanges)) notified++; };

        f.Events.Rows[0].IsEnabled = false;
        Assert.True(f.Events.HasUnsavedChanges);

        f.Events.SaveCommand.Execute(null);

        Assert.False(f.Events.HasUnsavedChanges);
        Assert.Equal(2, notified);
    }

    [Fact]
    public void UnsavedChanges_StayOnWhenSaveFails()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.SelectedRow = f.Events.Rows[0];
        f.Events.Description = "cambiata";
        f.Repository.SaveFailure = new IOException("bloccato");

        f.Events.SaveCommand.Execute(null);

        Assert.True(f.Events.HasUnsavedChanges);
    }

    [Fact]
    public void UnsavedChanges_AreSetByAddAndDelete_AndClearedByReload()
    {
        var f = new Fixture();
        f.Repository.Text = ScheduleFile.Format([Sample()]);
        f.Store.Reload();
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.EraseCommand.Execute(null);
        Assert.True(f.Events.HasUnsavedChanges);

        f.Store.Reload();
        Assert.False(f.Events.HasUnsavedChanges);
    }

    [Fact]
    public void SelectingARow_IsNotAChange()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);

        f.Events.SelectedRow = f.Events.Rows[0];

        Assert.False(f.Events.HasUnsavedChanges);
    }

    [Fact]
    public void RowEnabledText_FollowsTheCheckbox()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        var row = f.Events.Rows[0];
        Assert.Equal(Strings.Common_Yes, row.Enabled);

        row.IsEnabled = false;

        Assert.Equal(Strings.Common_No, row.Enabled);
    }

    [Fact]
    public void RowCheckbox_DisablesTheEventInMemoryWithoutSaving()
    {
        var f = new Fixture();
        f.Store.Replace([Sample("a"), Sample("b")]);

        f.Events.Rows[1].IsEnabled = false;

        Assert.False(f.Store.Events[1].Enabled);
        Assert.True(f.Store.Events[0].Enabled);
        Assert.Equal(0, f.Repository.Saves);
    }

    [Fact]
    public void RowCheckbox_OnTheSelectedRow_UpdatesTheForm()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.SelectedRow = f.Events.Rows[0];
        Assert.True(f.Events.IsEventEnabled);

        f.Events.Rows[0].IsEnabled = false;

        Assert.False(f.Events.IsEventEnabled);
        Assert.False(Assert.Single(f.Store.Events).Enabled);
    }

    [Fact]
    public void FormCheckbox_UpdatesTheRowCheckbox()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.IsEventEnabled = false;

        Assert.False(f.Events.Rows[0].IsEnabled);
    }

    [Fact]
    public void Row_ShowsTheIconOfItsUrl_AndFollowsUrlChanges()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        var row = f.Events.Rows[0];
        Assert.Equal("https://example.org", System.Text.Encoding.UTF8.GetString(row.Icon!));

        f.Events.SelectedRow = row;
        f.Events.Url = @"C:\doc.pdf";

        Assert.Equal(@"C:\doc.pdf", System.Text.Encoding.UTF8.GetString(row.Icon!));
    }

    [Fact]
    public async Task Firing_LaunchesTheDueEventAndLogsIt()
    {
        var f = new Fixture();
        f.Store.Replace([Sample("Lezione")]);

        await f.Firing.RunDueAsync(Now.AddMinutes(1));

        Assert.Equal(["https://example.org"], f.Launcher.Opened);
        Assert.Contains("Lezione", f.Log.Text);
        Assert.Equal("LaunchPlanner (Lezione)", f.Log.Status);
    }

    [Fact]
    public async Task Firing_DoesNotRepeatAfterTheProcessRestarts()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        await f.Firing.RunDueAsync(Now);

        var restarted = new FiringService(f.Store, f.Repository, f.Launcher, f.Log);
        await restarted.RunDueAsync(Now.AddMinutes(1));

        Assert.Single(f.Launcher.Opened);
    }

    [Fact]
    public async Task Startup_RelaunchesAnEventStillInsideItsDuration()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        await f.Firing.RunDueAsync(Now);

        var restarted = new FiringService(f.Store, f.Repository, f.Launcher, f.Log);
        await restarted.RunStartupAsync(Now.AddMinutes(10));

        Assert.Equal(2, f.Launcher.Opened.Count);
    }

    [Fact]
    public async Task Startup_DoesNotRelaunchAnEventWhoseDurationHasEnded()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        await f.Firing.RunDueAsync(Now);

        var restarted = new FiringService(f.Store, f.Repository, f.Launcher, f.Log);
        await restarted.RunStartupAsync(Now.AddMinutes(20));

        Assert.Single(f.Launcher.Opened);
    }

    [Fact]
    public async Task Firing_DeliveredOccurrenceIsNotRepeatedAfterAnAutomaticLaunch()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        await f.Firing.RunDueAsync(Now);

        await f.Firing.LaunchOccurrenceAsync(Sample(), Now);

        Assert.Single(f.Launcher.Opened);
    }

    [Fact]
    public async Task Firing_IgnoresAnOldNotificationTappedLater()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        await f.Firing.RunDueAsync(Now.AddDays(1));

        await f.Firing.LaunchOccurrenceAsync(Sample(), Now);

        Assert.Single(f.Launcher.Opened);
    }

    [Fact]
    public async Task Firing_LateAlarm_FiresWithinTheWiderWindow()
    {
        var f = new Fixture();
        f.Store.Replace([Sample() with { Duration = TimeSpan.Zero }]);

        await f.Firing.RunDueAsync(Now.AddMinutes(7), TimeSpan.FromMinutes(15));

        Assert.Single(f.Launcher.Opened);
    }

    [Fact]
    public async Task Firing_KeepsWorking_WhenTheFiredFileCannotBeRead()
    {
        var f = new Fixture();
        f.Repository.IsFiredLocked = true;
        f.Store.Replace([Sample()]);

        await f.Firing.RunDueAsync(Now);

        Assert.Single(f.Launcher.Opened);
    }

    [Fact]
    public async Task EditingAFiredEvent_DoesNotFireItAgain()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        await f.Firing.RunDueAsync(Now);
        f.Events.Refresh();
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.Url = "https://exa";
        f.Events.Description = "nuova";
        await f.Firing.RunDueAsync(Now.AddMinutes(1));

        Assert.Single(f.Launcher.Opened);
    }

    [Fact]
    public void Editor_SurvivesAHugeDuration()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.Refresh();
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.DurationDays = "99999999";

        Assert.True(f.Store.Events[0].Duration > TimeSpan.Zero);
    }

    [Fact]
    public void Store_RefusesToSaveAfterAFailedLoad()
    {
        var f = new Fixture();
        f.Repository.IsScheduleLocked = true;
        f.Store.Reload();
        f.Events.Refresh();

        f.Events.SaveCommand.Execute(null);

        Assert.Equal(0, f.Repository.Saves);
        Assert.Contains("Non è stato possibile", f.Events.Message);
    }

    [Fact]
    public void Store_DoesNotRetryAFailedLoadOnEveryCheck()
    {
        var f = new Fixture();
        f.Repository.IsScheduleLocked = true;
        var errors = 0;
        f.Log.ErrorReported += _ => errors++;

        f.Store.EnsureLoaded();
        f.Store.EnsureLoaded();

        Assert.Equal(1, errors);
    }

    [Fact]
    public async Task Firing_LogsAMissingUrl()
    {
        var f = new Fixture();
        f.Store.Replace([Sample() with { Url = "  " }]);

        await f.Firing.RunDueAsync(Now);

        Assert.Empty(f.Launcher.Opened);
        Assert.Contains("Manca l'URI", f.Log.Text);
    }

    [Fact]
    public async Task Firing_LogsALaunchFailureAndContinues()
    {
        var f = new Fixture();
        f.Launcher.Failure = "boom";
        f.Store.Replace([Sample()]);

        await f.Firing.RunDueAsync(Now);

        Assert.Contains("boom", f.Log.Text);
    }

    [Fact]
    public void Store_ReportsFileErrorsInTheLog()
    {
        var f = new Fixture();
        f.Repository.Text = "header\nriga cattiva\n";

        f.Store.Reload();

        Assert.Contains("riga 2", f.Log.Text);
    }

    [Fact]
    public void Editor_ShowsTheSelectedEventInTheForm()
    {
        var f = new Fixture();
        f.Store.Replace([Sample("Lezione") with { Duration = new TimeSpan(1, 2, 3, 4) }]);

        f.Events.Refresh();
        f.Events.SelectedRow = f.Events.Rows[0];

        Assert.Equal("Lezione", f.Events.Description);
        Assert.Equal("1", f.Events.DurationDays);
        Assert.Equal("2", f.Events.DurationHours);
        Assert.Equal("3", f.Events.DurationMinutes);
        Assert.Equal("4", f.Events.DurationSeconds);
        Assert.Equal(Periodicity.Day, f.Events.SelectedPeriod!.Period);
    }

    [Fact]
    public void Editor_ShowsEventsLoadedBeforeItWasCreated()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);

        var events = new EventsViewModel(f.Store, f.Launcher, f.Background, f.Shell, f.Log, f.Icons);

        Assert.Single(events.Rows);
    }

    [Fact]
    public void Editor_KeepsThePeriodOfTheSelectedRow_WhenAPickerClearsIt()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.Refresh();
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.SelectedPeriod = null;
        f.Events.Description = "nuova";

        Assert.Equal(Periodicity.Day, f.Events.SelectedPeriod?.Period);
        Assert.Equal("nuova", Assert.Single(f.Store.Events).Description);
    }

    [Fact]
    public void Editor_AppliesEditsToTheLiveListAtOnce()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.Refresh();
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.Description = "nuova";
        f.Events.DurationMinutes = "20";
        f.Events.StartTime = "08:30:15";
        f.Events.SelectedPeriod = PeriodOption.All.Single(o => o.Period == Periodicity.Monday);

        var ev = Assert.Single(f.Store.Events);
        Assert.Equal("nuova", ev.Description);
        Assert.Equal(TimeSpan.FromMinutes(20), ev.Duration);
        Assert.Equal(new DateTime(2026, 3, 9, 8, 30, 15), ev.Start);
        Assert.Equal(Periodicity.Monday, ev.Period);
        Assert.Equal("nuova", f.Events.Rows[0].Description);
        Assert.Equal(0, f.Repository.Saves);
    }

    [Fact]
    public void Editor_IgnoresAnInvalidTimeUntilItIsCorrected()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.Refresh();
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.StartTime = "25:";

        Assert.Equal(Now, f.Store.Events[0].Start);
    }

    [Fact]
    public void AddingReturnsToNewEventMode()
    {
        var f = new Fixture();
        f.Events.Refresh();
        f.Events.Description = "aggiunto";
        f.Events.StartTime = "10:20:30";
        f.Events.FinishTime = "10:20:30";
        f.Events.SelectedPeriod = PeriodOption.All.Single(o => o.Period == Periodicity.Hour);

        f.Events.AddCommand.Execute(null);

        Assert.Equal("aggiunto", Assert.Single(f.Store.Events).Description);
        Assert.Null(f.Events.SelectedRow);
        Assert.Null(f.Events.SelectedPeriod);
        Assert.Equal("", f.Events.Description);
        Assert.Equal("", f.Events.Url);
        Assert.Equal("", f.Events.StartTime);
        Assert.Equal("", f.Events.FinishTime);
    }

    [Fact]
    public void AddingRequiresAPeriod()
    {
        var f = new Fixture();
        f.Events.Refresh();

        f.Events.AddCommand.Execute(null);

        Assert.Empty(f.Store.Events);
        Assert.Equal("Definire la periodicità dell'evento", f.Events.Message);
    }

    [Fact]
    public void ClearingTheFormDeselectsAndResetsAllFields()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.Refresh();
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.ClearCommand.Execute(null);

        Assert.Null(f.Events.SelectedRow);
        Assert.Null(f.Events.SelectedPeriod);
        Assert.Equal("", f.Events.Description);
        Assert.Equal("", f.Events.Url);
        Assert.Equal("", f.Events.StartTime);
        Assert.Equal("", f.Events.FinishTime);
        Assert.Equal("0", f.Events.DurationDays);
        Assert.Equal("0", f.Events.DurationHours);
        Assert.Equal("0", f.Events.DurationMinutes);
        Assert.Equal("0", f.Events.DurationSeconds);
        Assert.True(f.Events.StartDate.HasValue);
        Assert.True(f.Events.FinishDate.HasValue);
        Assert.Equal(f.Events.StartDate, f.Events.FinishDate);
    }

    [Fact]
    public void NowOffsetUnit_DefaultsToMonths()
    {
        var f = new Fixture();

        Assert.Equal(OffsetUnit.Months, f.Events.SelectedNowOffsetUnit?.Unit);
    }

    [Fact]
    public void StartNow_SetsStartNow_AndEndWithOffset()
    {
        var f = new Fixture();
        f.Events.NowOffsetValue = "2";
        f.Events.SelectedNowOffsetUnit = NowOffsetUnit.All.Single(option => option.Unit == OffsetUnit.Hours);
        var beforeStart = DateTime.Now;
        var beforeFinish = beforeStart.AddHours(2);

        f.Events.StartNowCommand.Execute(null);

        var afterStart = DateTime.Now;
        var afterFinish = afterStart.AddHours(2);
        Assert.True(f.Events.StartDate.HasValue);
        Assert.True(f.Events.FinishDate.HasValue);

        var start = f.Events.StartDate!.Value.Date + TimeOnly.ParseExact(f.Events.StartTime, "HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture).ToTimeSpan();
        var finish = f.Events.FinishDate!.Value.Date + TimeOnly.ParseExact(f.Events.FinishTime, "HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture).ToTimeSpan();

        Assert.InRange(start, beforeStart.AddSeconds(-1), afterStart.AddSeconds(1));
        Assert.InRange(finish, beforeFinish.AddSeconds(-1), afterFinish.AddSeconds(1));
    }

    [Fact]
    public void StartNow_ShowsAnError_WhenOffsetIsInvalid()
    {
        var f = new Fixture();
        f.Events.NowOffsetValue = "abc";

        f.Events.StartNowCommand.Execute(null);

        Assert.Equal("Offset Ora non valido: inserire numero e unità.", f.Events.Message);
    }

    [Fact]
    public void ErasingRemovesTheSelectedEvent()
    {
        var f = new Fixture();
        f.Store.Replace([Sample("a"), Sample("b")]);
        f.Events.Refresh();
        f.Events.SelectedRow = f.Events.Rows[0];

        f.Events.EraseCommand.Execute(null);

        Assert.Equal("b", Assert.Single(f.Store.Events).Description);
        Assert.Null(f.Events.SelectedRow);
    }

    [Fact]
    public void SavingWritesTheFileAndRearmsTheBackgroundPart()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.Refresh();

        f.Events.SaveCommand.Execute(null);

        Assert.Equal(1, f.Repository.Saves);
        Assert.Equal(1, f.Background.Rearms);
    }

    [Fact]
    public void SavingWithoutEvents_AsksToAddFirst()
    {
        var f = new Fixture();

        f.Events.SaveCommand.Execute(null);

        Assert.Equal(0, f.Repository.Saves);
        Assert.Equal(0, f.Background.Rearms);
        Assert.Equal("Nessun evento da salvare: fare prima Aggiungi.", f.Events.Message);
    }

    [Fact]
    public void SavingWithoutEvents_HighlightsTheMessageAsError()
    {
        var f = new Fixture();

        f.Events.SaveCommand.Execute(null);

        Assert.True(f.Events.IsMessageError);
    }

    [Fact]
    public void SaveFailureOfAnyKind_IsShownAsErrorAndNotRearmed()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.Refresh();
        f.Repository.SaveFailure = new InvalidOperationException("causa imprevista");

        f.Events.SaveCommand.Execute(null);

        Assert.True(f.Events.IsMessageError);
        Assert.Contains("causa imprevista", f.Events.Message);
        Assert.Equal(0, f.Background.Rearms);
    }

    [Fact]
    public void MessageAfterASuccessfulSave_IsNotHighlighted()
    {
        var f = new Fixture();
        f.Store.Replace([Sample()]);
        f.Events.Refresh();
        f.Repository.SaveFailure = new IOException("bloccato");
        f.Events.SaveCommand.Execute(null);

        f.Repository.SaveFailure = null;
        f.Events.SaveCommand.Execute(null);

        Assert.False(f.Events.IsMessageError);
        Assert.Equal("", f.Events.Message);
    }

    [Fact]
    public void ValidationMessagesAreHighlightedToo()
    {
        var f = new Fixture();

        f.Events.AddCommand.Execute(null);

        Assert.Equal("Definire la periodicità dell'evento", f.Events.Message);
        Assert.True(f.Events.IsMessageError);
    }

    [Fact]
    public void EmptyMessageIsNotHighlighted()
    {
        var f = new Fixture();

        Assert.False(f.Events.IsMessageError);
    }

    [Fact]
    public async Task TestingLaunchesTheFormUrlWithoutMarkingItFired()
    {
        var f = new Fixture();
        f.Events.Url = "https://prova.it";

        await f.Events.TestCommand.ExecuteAsync(null);

        Assert.Equal(["https://prova.it"], f.Launcher.Opened);
    }

    [Fact]
    public async Task TestingWithoutUrlAsksForOne()
    {
        var f = new Fixture();

        await f.Events.TestCommand.ExecuteAsync(null);

        Assert.Equal("Inserire un URI o percorso file.", f.Events.Message);
    }

    [Fact]
    public void ClosingReturnsToTheShell()
    {
        var f = new Fixture();

        f.Events.CloseCommand.Execute(null);

        Assert.Equal(1, f.Shell.Closes);
    }

    [Fact]
    public void ReloadingFromTheLogPageReadsTheFileAndRearms()
    {
        var f = new Fixture();
        f.Repository.Save([Sample("dal file")]);
        var vm = new LogViewModel(f.Store, f.Log, f.Background, f.Shell);

        vm.ReloadCommand.Execute(null);

        Assert.Equal("dal file", Assert.Single(f.Store.Events).Description);
        Assert.Equal(1, f.Background.Rearms);
    }

    [Theory]
    [InlineData(" https://a.it ", "https://a.it")]
    [InlineData("file:///C:/Media/a.mp3", @"C:\Media\a.mp3")]
    [InlineData(@"C:\Media\a.mp3", @"C:\Media\a.mp3")]
    public void LaunchTarget_NormalizesTheAddress(string url, string expected)
    {
        if (!OperatingSystem.IsWindows() && url.StartsWith("file")) return;

        Assert.Equal(expected, LaunchTarget.Resolve(url));
    }

    [Fact]
    public void PeriodOptions_MapEachDayToItsOwnPeriod()
    {
        Assert.Equal(Periodicity.Monday, PeriodOption.All.Single(o => o.Caption == "lunedì").Period);
        Assert.Equal(Periodicity.Tuesday, PeriodOption.All.Single(o => o.Caption == "martedì").Period);
        Assert.Equal(Periodicity.Wednesday, PeriodOption.All.Single(o => o.Caption == "mercoledì").Period);
        Assert.Equal(PeriodOption.All.Count, PeriodOption.All.Select(o => o.Period).Distinct().Count());
    }
}
