using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Core.ViewModels;

public sealed partial class EventsViewModel : ObservableObject
{
    private const string TimeFormat = "HH:mm:ss";
    private const int MaximumDurationField = 9999; // i valori enormi non stanno in un TimeSpan
    private static readonly IReadOnlyList<string> nowOffsetUnits = ["s", "min", "h", "days", "months", "years"];

    private readonly EventStore store;
    private readonly IEventLauncher launcher;
    private readonly IBackgroundScheduler background;
    private readonly IShellActions shell;
    private readonly ActivityLog log;
    private bool isLoadingForm;

    public EventsViewModel(
        EventStore store, IEventLauncher launcher, IBackgroundScheduler background,
        IShellActions shell, ActivityLog log)
    {
        this.store = store;
        this.launcher = launcher;
        this.background = background;
        this.shell = shell;
        this.log = log;
        store.Changed += Refresh;
        Refresh();
        PrepareNewEventForm();
    }

    public ObservableCollection<EventRow> Rows { get; } = [];

    public IReadOnlyList<PeriodOption> Periods => PeriodOption.All;
    public IReadOnlyList<string> NowOffsetUnits => nowOffsetUnits;

    [ObservableProperty]
    public partial EventRow? SelectedRow { get; set; }

    [ObservableProperty]
    public partial DateTime? StartDate { get; set; }

    [ObservableProperty]
    public partial string StartTime { get; set; } = "";

    [ObservableProperty]
    public partial DateTime? FinishDate { get; set; }

    [ObservableProperty]
    public partial string FinishTime { get; set; } = "";

    [ObservableProperty]
    public partial string NowOffsetValue { get; set; } = "0";

    [ObservableProperty]
    public partial string? SelectedNowOffsetUnit { get; set; }

    [ObservableProperty]
    public partial string DurationDays { get; set; } = "0";

    [ObservableProperty]
    public partial string DurationHours { get; set; } = "0";

    [ObservableProperty]
    public partial string DurationMinutes { get; set; } = "0";

    [ObservableProperty]
    public partial string DurationSeconds { get; set; } = "0";

    [ObservableProperty]
    public partial PeriodOption? SelectedPeriod { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = "";

    [ObservableProperty]
    public partial string Url { get; set; } = "";

    [ObservableProperty]
    public partial bool IsEventEnabled { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = "";

    public void Refresh()
    {
        SelectedRow = null;
        Rows.Clear();
        foreach (var ev in store.Events)
            Rows.Add(new EventRow(ev));
    }

    partial void OnSelectedRowChanged(EventRow? value)
    {
        if (value is not null)
            FillForm(value.Event);
    }

    partial void OnStartDateChanged(DateTime? value) => ApplyForm();
    partial void OnStartTimeChanged(string value) => ApplyForm();
    partial void OnFinishDateChanged(DateTime? value) => ApplyForm();
    partial void OnFinishTimeChanged(string value) => ApplyForm();
    partial void OnDurationDaysChanged(string value) => ApplyForm();
    partial void OnDurationHoursChanged(string value) => ApplyForm();
    partial void OnDurationMinutesChanged(string value) => ApplyForm();
    partial void OnDurationSecondsChanged(string value) => ApplyForm();

    partial void OnSelectedPeriodChanged(PeriodOption? value)
    {
        // Il selettore di una pagina sostituita può azzerarsi: la riga selezionata ha sempre un periodo
        if (value is null && SelectedRow is not null)
            SelectedPeriod = PeriodOption.All.First(option => option.Period == SelectedRow.Event.Period);
        else
            ApplyForm();
    }

    partial void OnDescriptionChanged(string value) => ApplyForm();
    partial void OnUrlChanged(string value) => ApplyForm();
    partial void OnIsEventEnabledChanged(bool value) => ApplyForm();

    [RelayCommand]
    private void Add()
    {
        if (SelectedPeriod is null)
        {
            Message = Strings.Error_PeriodRequired;
            return;
        }
        if (ReadForm(SelectedPeriod.Period) is not { } ev)
        {
            Message = Strings.Error_InvalidTime;
            return;
        }

        Message = "";
        store.Add(ev);
        PrepareNewEventForm();
    }

    [RelayCommand]
    private void Clear()
    {
        Message = "";
        PrepareNewEventForm();
    }

    [RelayCommand]
    private void Erase()
    {
        if (SelectedRow is not null)
            store.RemoveAt(Rows.IndexOf(SelectedRow));
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            store.Save();
            Message = "";
            background.Rearm();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Message = string.Format(Strings.Error_CannotWriteFile, ex.Message);
            log.Add(Message);
        }
    }

    [RelayCommand]
    private async Task TestAsync()
    {
        if (LaunchTarget.Resolve(Url) is null)
        {
            Message = Strings.Error_UrlRequired;
            return;
        }

        try
        {
            Message = "";
            await launcher.LaunchAsync(new ScheduledEvent { Description = Description, Url = Url });
        }
        catch (Exception ex)
        {
            Message = string.Format(Strings.Error_CannotOpenTarget, ex.Message);
        }
    }

    [RelayCommand]
    private void Close() => shell.CloseEditor();

    [RelayCommand]
    private void StartNow()
    {
        if (!TryReadNowOffset(out var amount, out var unit))
        {
            Message = Strings.Error_InvalidNowOffset;
            return;
        }

        var now = DateTime.Now;
        if (ApplyNowOffset(now, amount, unit) is not { } finish)
        {
            Message = Strings.Error_InvalidNowOffset;
            return;
        }

        StartDate = now.Date;
        StartTime = now.ToString(TimeFormat, CultureInfo.InvariantCulture);
        FinishDate = finish.Date;
        FinishTime = finish.ToString(TimeFormat, CultureInfo.InvariantCulture);
        Message = "";
    }

    private void PrepareNewEventForm()
    {
        var now = DateTime.Now;
        isLoadingForm = true;
        SelectedRow = null;
        StartDate = now.Date;
        StartTime = "";
        FinishDate = now.Date;
        FinishTime = "";
        NowOffsetValue = "0";
        SelectedNowOffsetUnit = nowOffsetUnits[1];
        DurationDays = "0";
        DurationHours = "0";
        DurationMinutes = "0";
        DurationSeconds = "0";
        SelectedPeriod = null;
        Description = "";
        Url = "";
        IsEventEnabled = true;
        isLoadingForm = false;
    }

    private void FillForm(ScheduledEvent ev)
    {
        isLoadingForm = true;
        StartDate = ev.Start.Date;
        StartTime = ev.Start.ToString(TimeFormat, CultureInfo.InvariantCulture);
        FinishDate = ev.Finish.Date;
        FinishTime = ev.Finish.ToString(TimeFormat, CultureInfo.InvariantCulture);
        DurationDays = ev.Duration.Days.ToString(CultureInfo.InvariantCulture);
        DurationHours = ev.Duration.Hours.ToString(CultureInfo.InvariantCulture);
        DurationMinutes = ev.Duration.Minutes.ToString(CultureInfo.InvariantCulture);
        DurationSeconds = ev.Duration.Seconds.ToString(CultureInfo.InvariantCulture);
        SelectedPeriod = PeriodOption.All.First(option => option.Period == ev.Period);
        Description = ev.Description;
        Url = ev.Url;
        IsEventEnabled = ev.Enabled;
        isLoadingForm = false;
    }

    // La riga selezionata segue il modulo a ogni modifica, come in LaunchPlanner_Forms
    private void ApplyForm()
    {
        if (isLoadingForm || SelectedRow is null || SelectedPeriod is null)
            return;
        if (ReadForm(SelectedPeriod.Period) is not { } ev)
            return;

        store.Update(Rows.IndexOf(SelectedRow), ev);
        SelectedRow.Event = ev;
    }

    private ScheduledEvent? ReadForm(Periodicity period)
    {
        if (Combine(StartDate, StartTime) is not { } start || Combine(FinishDate, FinishTime) is not { } finish)
            return null;

        return new ScheduledEvent
        {
            Start = start,
            Finish = finish,
            Duration = new TimeSpan(Number(DurationDays), Number(DurationHours), Number(DurationMinutes), Number(DurationSeconds)),
            Period = period,
            Description = Description,
            Url = Url,
            Enabled = IsEventEnabled,
        };
    }

    private static DateTime? Combine(DateTime? date, string time)
        => date is { } day && TimeOnly.TryParse(time, CultureInfo.InvariantCulture, out var clock)
            ? day.Date + clock.ToTimeSpan()
            : null;

    private bool TryReadNowOffset(out int amount, out string unit)
    {
        amount = 0;
        unit = "";

        if (SelectedNowOffsetUnit is null || !nowOffsetUnits.Contains(SelectedNowOffsetUnit))
            return false;
        if (!int.TryParse(NowOffsetValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            return false;

        amount = Math.Clamp(value, 0, MaximumDurationField);
        unit = SelectedNowOffsetUnit;
        return true;
    }

    private static DateTime? ApplyNowOffset(DateTime value, int amount, string unit)
    {
        try
        {
            return unit switch
            {
                "s" => value.AddSeconds(amount),
                "min" => value.AddMinutes(amount),
                "h" => value.AddHours(amount),
                "days" => value.AddDays(amount),
                "months" => value.AddMonths(amount),
                "years" => value.AddYears(amount),
                _ => null,
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static int Number(string text)
        => int.TryParse(text, out var value) ? Math.Clamp(value, 0, MaximumDurationField) : 0;
}
