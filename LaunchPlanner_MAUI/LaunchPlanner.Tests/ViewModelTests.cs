using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Persistence;
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
        public void Save(IReadOnlyList<ScheduledEvent> events) { Text = ScheduleFile.Format(events); Saves++; }
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

    private sealed class Fixture
    {
        public FakeRepository Repository { get; } = new();
        public FakeLauncher Launcher { get; } = new();
        public FakeBackground Background { get; } = new();
        public FakeShell Shell { get; } = new();
        public ActivityLog Log { get; } = new();
        public EventStore Store { get; }
        public FiringService Firing { get; }
        public EventsViewModel Events { get; }

        public Fixture()
        {
            Store = new EventStore(Repository, Log);
            Firing = new FiringService(Store, Repository, Launcher, Log);
            Events = new EventsViewModel(Store, Launcher, Background, Shell, Log);
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

        var events = new EventsViewModel(f.Store, f.Launcher, f.Background, f.Shell, f.Log);

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
    public void AddingSelectsTheNewEvent()
    {
        var f = new Fixture();
        f.Events.Refresh();
        f.Events.Description = "aggiunto";
        f.Events.SelectedPeriod = PeriodOption.All.Single(o => o.Period == Periodicity.Hour);

        f.Events.AddCommand.Execute(null);

        Assert.Equal("aggiunto", Assert.Single(f.Store.Events).Description);
        Assert.Same(f.Events.Rows[0], f.Events.SelectedRow);
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
