using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Persistence;

namespace LaunchPlanner.Tests;

public sealed class ScheduleFileTests
{
    private const string Header = "Istante Inizio\tIstante Fine\tDurata\tPeriodicità\tDescrizione\tURL\tAbilitato\r\n";

    [Fact]
    public void ReadsARowWrittenByTheWindowsFormsProgram()
    {
        var text = Header + "02/10/2018 14:50:00\t10/06/2030 15:05:00\t00:15:00\tWeekday\tLezione\thttps://a.it\tTrue\r\n";

        var result = ScheduleFile.Parse(text);

        Assert.Empty(result.Errors);
        var ev = Assert.Single(result.Events);
        Assert.Equal(new DateTime(2018, 10, 2, 14, 50, 0), ev.Start);
        Assert.Equal(new DateTime(2030, 6, 10, 15, 5, 0), ev.Finish);
        Assert.Equal(TimeSpan.FromMinutes(15), ev.Duration);
        Assert.Equal(Periodicity.Weekday, ev.Period);
        Assert.Equal("Lezione", ev.Description);
        Assert.Equal("https://a.it", ev.Url);
        Assert.True(ev.Enabled);
    }

    [Theory]
    [InlineData("2/10/2018 14.50.0")]
    [InlineData("02/10/2018 14.50.00")]
    [InlineData("2018-10-02 14:50:00")]
    [InlineData("2018-10-02T14:50:00")]
    public void AcceptsTheOldAlternativeDateFormats(string date)
    {
        var text = Header + $"{date}\t10/06/2030\t00:15:00\tDay\td\tu\tTrue";

        Assert.Equal(new DateTime(2018, 10, 2, 14, 50, 0), Assert.Single(ScheduleFile.Parse(text).Events).Start);
    }

    [Fact]
    public void ReadsDurationsWithDays()
    {
        var text = Header + "02/10/2018 14:50:00\t10/06/2030 15:05:00\t1.02:03:04\tDay\td\tu\tFalse";

        Assert.Equal(new TimeSpan(1, 2, 3, 4), Assert.Single(ScheduleFile.Parse(text).Events).Duration);
    }

    [Fact]
    public void ReportsADurationTooLargeForATimeSpan()
    {
        var text = Header + "02/10/2018 14:50:00\t10/06/2030 15:05:00\t99999999.00:00:00\tDay\td\tu\tTrue";

        var result = ScheduleFile.Parse(text);

        Assert.Empty(result.Events);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void SkipsCommentsAndReportsBadRowsWithoutLosingTheGoodOnes()
    {
        var text = Header
            + "'commento\r\n"
            + "non una data\t10/06/2030\t00:15:00\tDay\td\tu\tTrue\r\n"
            + "riga corta\r\n"
            + "02/10/2018 14:50:00\t10/06/2030\tx\tDay\td\tu\tTrue\r\n"
            + "02/10/2018 14:50:00\t10/06/2030\t00:15:00\tDay\tbuona\tu\tTrue\r\n";

        var result = ScheduleFile.Parse(text);

        Assert.Equal("buona", Assert.Single(result.Events).Description);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains("riga 3", result.Errors[0]);
    }

    [Fact]
    public void FormatThenParse_PreservesEveryEvent()
    {
        ScheduledEvent[] events =
        [
            new()
            {
                Start = new DateTime(2018, 7, 4, 21, 10, 0),
                Finish = new DateTime(2030, 11, 28, 23, 38, 17),
                Duration = new TimeSpan(1, 0, 3, 0),
                Period = Periodicity.QuarterOfHour,
                Description = "Più lungo",
                Url = @"C:\Media\file.mp3",
                Enabled = false,
            },
        ];

        var result = ScheduleFile.Parse(ScheduleFile.Format(events));

        Assert.Equal(events, result.Events);
    }

    [Fact]
    public void Format_KeepsTheColumnsOfTheWindowsFormsFile()
    {
        var text = ScheduleFile.Format([new ScheduledEvent
        {
            Start = new DateTime(2018, 10, 2, 14, 50, 0),
            Finish = new DateTime(2030, 6, 10, 15, 5, 0),
            Duration = TimeSpan.FromMinutes(15),
            Period = Periodicity.Weekday,
            Description = "con\ttab",
            Url = "u",
            Enabled = true,
        }]);

        Assert.Equal(Header + "02/10/2018 14:50:00\t10/06/2030 15:05:00\t00:15:00\tWeekday\tcon tab\tu\tTrue\r\n", text);
    }

    [Theory]
    [InlineData("nessuno", Periodicity.NonPeriodical)]
    [InlineData("SingleShot", Periodicity.SingleShot)]
    [InlineData("secondo", Periodicity.Second)]
    [InlineData("minuto", Periodicity.OneMinute)]
    [InlineData("OneMinute", Periodicity.OneMinute)]
    [InlineData("quartodora", Periodicity.QuarterOfHour)]
    [InlineData("mezzora", Periodicity.HalfHour)]
    [InlineData("ora", Periodicity.Hour)]
    [InlineData("HOUR", Periodicity.Hour)]
    [InlineData("giorno", Periodicity.Day)]
    [InlineData("feriali", Periodicity.Weekday)]
    [InlineData("settimana", Periodicity.Weekly)]
    [InlineData("Weekly", Periodicity.Weekly)]
    [InlineData("finesettimana", Periodicity.Weekend)]
    [InlineData("lunedì", Periodicity.Monday)]
    [InlineData("domenica", Periodicity.Sunday)]
    [InlineData("mese", Periodicity.Month)]
    [InlineData("anno", Periodicity.Year)]
    [InlineData("personalizzato", Periodicity.Custom)]
    [InlineData("  ", Periodicity.NonPeriodical)]
    [InlineData("sconosciuto", Periodicity.NonPeriodical)]
    public void ParsesPeriodNames(string name, Periodicity expected)
    {
        Assert.Equal(expected, ScheduleFile.ParsePeriod(name));
    }

    [Fact]
    public void EveryPeriodSurvivesARoundTripByName()
    {
        foreach (var period in Enum.GetValues<Periodicity>())
        {
            Assert.Equal(period, ScheduleFile.ParsePeriod(period.ToString()));
        }
    }
}
