using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Core.ViewModels;

public sealed class EventRow(ScheduledEvent ev, IEventIconProvider? icons = null) : ObservableObject
{
    private const string GeneralDateFormat = "G"; // data e ora nel formato dell'utente

    // Solo per i cambi fatti dalla casella della riga, non per quelli che arrivano dal modulo
    public event Action<EventRow>? EnabledToggled;

    public ScheduledEvent Event
    {
        get => ev;
        set
        {
            ev = value;
            OnPropertyChanged(string.Empty);
        }
    }

    public bool IsEnabled
    {
        get => ev.Enabled;
        set
        {
            if (value == ev.Enabled)
                return;

            Event = ev with { Enabled = value };
            EnabledToggled?.Invoke(this);
        }
    }

    // Letta a ogni aggiornamento della riga: segue i cambi dell'URL fatti dal modulo
    public byte[]? Icon => icons?.GetIconPng(ev.Url);

    public string Start => ev.Start.ToString(GeneralDateFormat, CultureInfo.CurrentCulture);
    public string Finish => ev.Finish.ToString(GeneralDateFormat, CultureInfo.CurrentCulture);
    public string Duration => ev.Duration.ToString("c", CultureInfo.InvariantCulture);
    public string Period => PeriodOption.All.First(option => option.Period == ev.Period).Caption;

    // Testo "Abilitato: Sì/No": si aggiorna insieme alla casella, perché cambia l'evento
    public string Enabled => ev.Enabled ? Strings.Common_Yes : Strings.Common_No;
    public string Description => ev.Description;
    public string Url => ev.Url;
}
