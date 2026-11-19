using System.Globalization;
using System.Text;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;

namespace LaunchPlanner.Core.Persistence;

public sealed record ScheduleParseResult(IReadOnlyList<ScheduledEvent> Events, IReadOnlyList<string> Errors);

// Stesso file di testo di LaunchPlanner_Forms: una riga di intestazione e poi una riga per evento,
// con i campi separati da tabulazioni. Le righe che iniziano con ' sono commenti.
// Il formato del file non dipende dalla lingua dell'interfaccia: qui si traducono solo i messaggi di errore.
public static class ScheduleFile
{
    private const char Separator = '\t';
    private const string Comment = "'";
    private const int ColumnCount = 7;
    private const string DateFormat = "dd/MM/yyyy HH:mm:ss";
    private const string Header = "Istante Inizio\tIstante Fine\tDurata\tPeriodicità\tDescrizione\tURL\tAbilitato";
    private const string LineEnd = "\r\n";

    private static readonly string[] AcceptedDateFormats =
    [
        "dd/MM/yyyy HH.mm.ss",
        "d/M/yyyy H.m.s",
        "dd/MM/yyyy H.mm.ss",
        DateFormat,
        "d/M/yyyy H:m:s",
        "dd/MM/yyyy H:m:s",
        "dd/MM/yyyy",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
    ];

    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");

    public static ScheduleParseResult Parse(string text)
    {
        var events = new List<ScheduledEvent>();
        var errors = new List<string>();
        var lines = text.ReplaceLineEndings("\n").Split('\n');

        for (var i = 1; i < lines.Length; i++) // la riga 0 è l'intestazione
        {
            if (lines[i].Length == 0 || lines[i].StartsWith(Comment, StringComparison.Ordinal))
                continue;

            if (TryParseLine(lines[i], out var ev, out var problem))
                events.Add(ev);
            else
                errors.Add(string.Format(Strings.Schedule_LineError, problem, i + 1));
        }
        return new ScheduleParseResult(events, errors);
    }

    public static string Format(IEnumerable<ScheduledEvent> events)
    {
        var text = new StringBuilder(Header).Append(LineEnd);
        foreach (var ev in events)
        {
            string[] fields =
            [
                ev.Start.ToString(DateFormat, CultureInfo.InvariantCulture),
                ev.Finish.ToString(DateFormat, CultureInfo.InvariantCulture),
                ev.Duration.ToString("c", CultureInfo.InvariantCulture),
                ev.Period.ToString(),
                ev.Description.Replace(Separator, ' '),
                ev.Url.Replace(Separator, ' '),
                ev.Enabled.ToString(),
            ];
            text.AppendJoin(Separator, fields).Append(LineEnd);
        }
        return text.ToString();
    }

    public static Periodicity ParsePeriod(string name)
    {
        var key = name.Trim().ToLowerInvariant();
        return (key.Length > 5 ? key[..5] : key) switch
        {
            "singl" => Periodicity.SingleShot,
            "secon" => Periodicity.Second,
            "minut" or "onemi" => Periodicity.OneMinute,
            "quart" => Periodicity.QuarterOfHour,
            "mezzo" or "halfh" => Periodicity.HalfHour,
            "ora" or "hour" => Periodicity.Hour,
            "giorn" or "day" => Periodicity.Day,
            "feria" or "weekd" => Periodicity.Weekday,
            "week" or "setti" or "weekl" => Periodicity.Weekly,
            "fines" or "weeke" => Periodicity.Weekend,
            "luned" or "monda" => Periodicity.Monday,
            "marte" or "tuesd" => Periodicity.Tuesday,
            "merco" or "wedne" => Periodicity.Wednesday,
            "giove" or "thurs" => Periodicity.Thursday,
            "vener" or "frida" => Periodicity.Friday,
            "sabat" or "satur" => Periodicity.Saturday,
            "domen" or "sunda" => Periodicity.Sunday,
            "mese" or "month" => Periodicity.Month,
            "anno" or "year" => Periodicity.Year,
            "custo" or "perso" => Periodicity.Custom,
            _ => Periodicity.NonPeriodical,
        };
    }

    private static bool TryParseLine(string line, out ScheduledEvent ev, out string problem)
    {
        ev = new ScheduledEvent();
        problem = "";
        var fields = line.Split(Separator);
        if (fields.Length < ColumnCount)
            problem = Strings.Schedule_TooFewFields;
        else if (!TryParseDate(fields[0], out var start))
            problem = string.Format(Strings.Schedule_InvalidStart, fields[0]);
        else if (!TryParseDate(fields[1], out var finish))
            problem = string.Format(Strings.Schedule_InvalidFinish, fields[1]);
        else if (!TryParseDuration(fields[2], out var duration))
            problem = string.Format(Strings.Schedule_InvalidDuration, fields[2]);
        else if (!bool.TryParse(fields[6].Trim(), out var enabled))
            problem = string.Format(Strings.Schedule_InvalidEnabled, fields[6]);
        else
            ev = new ScheduledEvent
            {
                Start = start,
                Finish = finish,
                Duration = duration,
                Period = ParsePeriod(fields[3]),
                Description = fields[4],
                Url = fields[5],
                Enabled = enabled,
            };
        return problem.Length == 0;
    }

    private static bool TryParseDate(string text, out DateTime date)
    {
        var raw = text.Trim();
        return DateTime.TryParseExact(raw, AcceptedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date)
            || DateTime.TryParse(raw.Replace('.', ':'), Italian, DateTimeStyles.AllowWhiteSpaces, out date);
    }

    // Formato [giorni.]ore:minuti:secondi; a differenza di TimeSpan.Parse accetta anche ore oltre 23
    private static bool TryParseDuration(string text, out TimeSpan duration)
    {
        duration = default;
        var parts = text.Trim().Split(':');
        if (parts.Length != 3)
            return false;

        var daysAndHours = parts[0].Split('.');
        var days = 0;
        if (daysAndHours.Length > 2
            || (daysAndHours.Length == 2 && !int.TryParse(daysAndHours[0], out days))
            || !int.TryParse(daysAndHours[^1], out var hours)
            || !int.TryParse(parts[1], out var minutes)
            || !int.TryParse(parts[2], out var seconds))
            return false;

        try
        {
            duration = new TimeSpan(days, hours, minutes, seconds);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
