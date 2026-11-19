using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.ViewModels;

public sealed class EventRow(ScheduledEvent ev) : ObservableObject
{
    private const string GeneralDateFormat = "G"; // data e ora nel formato dell'utente

    public ScheduledEvent Event
    {
        get => ev;
        set
        {
            ev = value;
            OnPropertyChanged(string.Empty);
        }
    }

    public string Start => ev.Start.ToString(GeneralDateFormat, CultureInfo.CurrentCulture);
    public string Finish => ev.Finish.ToString(GeneralDateFormat, CultureInfo.CurrentCulture);
    public string Duration => ev.Duration.ToString("c", CultureInfo.InvariantCulture);
    public string Period => PeriodOption.All.First(option => option.Period == ev.Period).Caption;
    public string Description => ev.Description;
    public string Url => ev.Url;
    public string Enabled => ev.Enabled ? Strings.Common_Yes : Strings.Common_No;
}
