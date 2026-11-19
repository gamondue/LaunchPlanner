using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace gamon.LaunchPlanner
{
    public partial class frmBackground : Form
    {
#if DEBUG
        //string nomeFileDati = "prova.txt";
        string nomeFileDati = "LaunchPlanner schedule_DEBUG.txt";
#else
        string nomeFileDati = "LaunchPlanner schedule.txt";
#endif
        // Percorso della cartella Documenti\LaunchPlanner dell'utente (inserito nel ctor)
        string pathEFileDati;

        // ELENCO delle Periodicità implementate attualmente:
        // N.B. NEL CAMPO "Periodo" DEL FILE SI DEVONO USARE LE PRIME QUATTRO LETTERE, non importa se maiuscole o minuscole.
        // Oltre le prime quattro lettere il programma non controlla, per cui si può scrivere quello che si vuole. 

        // nessuno  evento singolo, sparato una sola volta, non periodico
        // zero  evento singolo, sparato una sola volta, non periodico

        // minuto   evento sparato ogni minuto (oppure al primo minuto disponibile dopo la fine della sua durata, se la durata è superiore al minuto)
        // quartodora
        // mezzora

        // ora      evento sparato ad ogni ora, al minuto ed al secondo indicato dall'istante di inizio dell'evento
        // giorno   evento sparato ogni giorno, all'ora, minuto e secondo indicato dall'istante di inizio dell'evento
        // feriali  evento sparato da Lunedì a Venerdì, all'ora, minuto e secondo indicato dall'istante di inizio dell'evento
        // weekend o finesettimana  evento sparato Sabato e Domenica, all'ora, minuto e secondo indicato dall'istante di inizio dell'evento
        // lunedì   evento sparato il Lunedì, all'ora, minuto e secondo indicato dall'istante di inizio dell'evento
        // martedì  vedi sopra, ma con Martedì
        // mercoledì vedi sopra
        // giovedì  vedi sopra
        // venerdì  vedi sopra
        // sabato   vedi sopra
        // domenica vedi sopra

        // mese     evento sparato ad ogni mese, nel giorno, ora, minuto e secondo indicato dall'istante di inizio dell'evento

        // anno     evento sparato ad ogni anno, nel mese, giorno, ora, minuto e secondo indicato dall'istante di inizio dell'evento

        // CASO "GENERALE":
        // stringa che rappresenta un tempo QUALSIASI, espresso come: 
        //
        // numAnni-numMesi-numGiorni ore:minuti:secondi
        //
        // L'evento viene sparato dopo ogni anno, mese, giorno, ora, minuto e secondo, come indicato, a partire dall'istante di inizio dell'evento
        // Non si può "abbreviare" la scrittura precedente, se si vuole che alcuni dei tempi richiesti siano nulli, mettere zero.
        // Esempio: se si vuole un periodo di sei ore e 30 secondi, scrivere 0-0-0 6:0:30; 
        //          se l'instante di Inizio è "2006-09-27 14:00:00", l'evento partirà alle:
        //          2006-09-27 14:00:00
        //          2006-09-27 14:06:30
        //          2006-09-27 14:13:00 ..
        //
        // N.B. il programma ci mette più tempo ad elaborare una periodicità qualsiasi; se si può usare una delle altre

        //TODO Elenco delle Periodicità ancora da fare 
        //    secondo
        //    festivi (cercare funzione che abbia i festivi in Italia o all'estero)
        //    settimana
        //    stagione
        //    semestre
        //    lustro

        //public Evento[] eventi = new Evento[0];
        //public ArrayList eventi = new ArrayList();
        List<Event> eventi = new List<Event>();

        public frmBackground()
        {
            InitializeComponent();

            // Imposta la cartella Documenti\<utente>\LaunchPlanner e la crea se non esiste.
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string launchDir = Path.Combine(documents, "LaunchPlanner");
            try
            {
                Directory.CreateDirectory(launchDir);
            }
            catch
            {
                // Se la creazione fallisce, fallback alla cartella corrente per non bloccare l'app.
                launchDir = ".";
            }
            pathEFileDati = Path.Combine(launchDir, nomeFileDati);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            // Show the form when the user double clicks on the notify icon.

            // Set the WindowState to normal if the form is minimized.
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.Visible = true;

                // Activate the form.
                this.Activate();
            }
            else
            {
                MinimizeInTray();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (endSessionPending)
            {
                // The session is ending.
                e.Cancel = false;
            }
            else
            {
                // The window must only be minimized in tray
                e.Cancel = true;
                MinimizeInTray();
                this.Visible = false;
            }
            base.OnClosing(e);
        }

        private void MinimizeInTray()
        {
            this.ShowInTaskbar = false;
            this.Visible = false;
            this.WindowState = FormWindowState.Minimized;
        }

        private const int WM_QUERYENDSESSION = 0x11;
        private bool endSessionPending;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_QUERYENDSESSION)
                endSessionPending = true;
            base.WndProc(ref m);
        }
        private void uscitaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void uscitaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //DateTime StartFire;
            //DateTime FinishFire;
            //DateTime NextFire;
            DateTime Now = DateTime.Now;

            String[] temp;
            String[] day;
            String[] hour;

            for (int i = 0; i < eventi.Count; i++)
            {
                Event ev = (Event)eventi[i];

                if (ev.Enabled)
                {
                    // se siamo fuori dal tempo "globale" dell'evento, non lo spara comunque
                    if (Now >= ev.StartAll && Now <= ev.FinishAll)
                    {
                        DateTime nextE = NextEventTime(ev);
                        if (nextE == DateTime.MaxValue)
                            continue;
                        if (Now >= nextE && Now <= nextE + ev.Duration && !ev.AlreadyFired)
                            accendiURL(ev);
                        if (Now > nextE + ev.Duration)
                            ev.AlreadyFired = false;
                    }
                }
            }
        }
        private DateTime NextEventTime(Event Event)
        {
            DateTime nextEvent = DateTime.MaxValue;
            DateTime Now = DateTime.Now;
            // calcola, in base alla periodicità dell'evento, la sua prossima occorrenza
            //Event.PeriodType controllo = Event.PeriodType.ToLower();
            //if (controllo.Length > 3) controllo = controllo.Substring(0, 4);
            switch (Event.PeriodType)
            {
                case Event.Periodicity.SingleShot:
                case Event.Periodicity.NonPeriodical:
                    {
                        nextEvent = Event.StartAll;
                        break;
                    }
                case Event.Periodicity.OneMinute:
                    {
                        // minuto
                        nextEvent = new DateTime(Now.Year, Now.Month, Now.Day, Now.Hour,
                                                    Now.Minute, Event.StartAll.Second);
                        break;
                    }
                case Event.Periodicity.QuarterOfHour:
                    nextEvent = latestOccurrence(Event, new TimeSpan(0, 15, 0), Now);
                    break;

                case Event.Periodicity.HalfHour:
                    nextEvent = latestOccurrence(Event, new TimeSpan(0, 30, 0), Now);
                    break;

                case Event.Periodicity.Hour:
                    nextEvent = new DateTime(Now.Year, Now.Month, Now.Day, Now.Hour,
                                                Event.StartAll.Minute, Event.StartAll.Second);
                    break;

                case Event.Periodicity.Day:
                    nextEvent = new DateTime(Now.Year, Now.Month, Now.Day, Event.StartAll.Hour,
                                                Event.StartAll.Minute, Event.StartAll.Second);
                    break;

                case Event.Periodicity.Weekday:
                    {
                        nextEvent = new DateTime(Now.Year, Now.Month, Now.Day, Event.StartAll.Hour,
                                                Event.StartAll.Minute, Event.StartAll.Second);
                        if (nextEvent.DayOfWeek == DayOfWeek.Saturday || nextEvent.DayOfWeek == DayOfWeek.Sunday)
                            nextEvent = DateTime.MaxValue;
                        break;
                    }

                case Event.Periodicity.Weekly:
                    nextEvent = latestOccurrence(Event, new TimeSpan(7, 0, 0, 0), Now);
                    break;

                case Event.Periodicity.Weekend:
                    nextEvent = new DateTime(Now.Year, Now.Month, Now.Day, Event.StartAll.Hour,
                                                Event.StartAll.Minute, Event.StartAll.Second);
                    if (nextEvent.DayOfWeek != DayOfWeek.Saturday && nextEvent.DayOfWeek != DayOfWeek.Sunday)
                    {
                        nextEvent = DateTime.MaxValue;
                    }
                    break;

                case Event.Periodicity.Monday:
                    nextEvent = nextTimeSameWeekDay(DayOfWeek.Monday, Now, Event);
                    break;

                case Event.Periodicity.Tuesday:
                    nextEvent = nextTimeSameWeekDay(DayOfWeek.Tuesday, Now, Event);
                    break;

                case Event.Periodicity.Wednesday:
                    nextEvent = nextTimeSameWeekDay(DayOfWeek.Wednesday, Now, Event);
                    break;

                case Event.Periodicity.Thursday:
                    nextEvent = nextTimeSameWeekDay(DayOfWeek.Thursday, Now, Event);
                    break;

                case Event.Periodicity.Friday:
                    nextEvent = nextTimeSameWeekDay(DayOfWeek.Friday, Now, Event);
                    break;

                case Event.Periodicity.Saturday:
                    nextEvent = nextTimeSameWeekDay(DayOfWeek.Saturday, Now, Event);
                    break;

                case Event.Periodicity.Sunday:
                    nextEvent = nextTimeSameWeekDay(DayOfWeek.Sunday, Now, Event);
                    break;

                case Event.Periodicity.Month:
                    nextEvent = sameDayOfMonth(Event, Now);
                    break;

                case Event.Periodicity.Year:
                    nextEvent = Event.StartAll.AddYears(Now.Year - Event.StartAll.Year);
                    break;

                case Event.Periodicity.Custom:
                    // the custom period is read from the Duration data
                    if (Event.Duration <= new TimeSpan(0, 0, 1))
                        nextEvent = new DateTime(Now.Year, Now.Month, Now.Day, Now.Hour,
                                Now.Minute, Event.StartAll.Second);
                    else if (Event.Duration <= new TimeSpan(0, 1, 0))
                        nextEvent = new DateTime(Now.Year, Now.Month, Now.Day, Now.Hour,
                                Event.StartAll.Minute, Event.StartAll.Second);
                    else if (Event.Duration <= new TimeSpan(1, 0, 0))
                        nextEvent = new DateTime(Now.Year, Now.Month, Now.Day, Event.StartAll.Hour,
                                Event.StartAll.Minute, Event.StartAll.Second);
                    else if (Event.Duration <= new TimeSpan(1, 0, 0, 0))
                        nextEvent = sameDayOfMonth(Event, Now);
                    break;

                default:
                    nextEvent = DateTime.MaxValue;
                    break;
            }
            return nextEvent;
        }
        // ultima occorrenza non successiva a Now di un evento con periodo fisso, allineata a StartAll
        private static DateTime latestOccurrence(Event Event, TimeSpan Period, DateTime Now)
        {
            long elapsedPeriods = (Now - Event.StartAll).Ticks / Period.Ticks;
            return Event.StartAll.AddTicks(elapsedPeriods * Period.Ticks);
        }
        // occorrenza del mese corrente: AddMonths porta all'ultimo giorno i mesi troppo corti
        private static DateTime sameDayOfMonth(Event Event, DateTime Now)
        {
            int months = (Now.Year - Event.StartAll.Year) * 12 + Now.Month - Event.StartAll.Month;
            return Event.StartAll.AddMonths(months);
        }
        //private void accendiURL(DateTime ProssimoFire, DateTime Adesso, ref Event e)
        private void accendiURL(Event e)
        {
            //DateTime FineFire;
            //DateTime FirePrecedente;
            //DateTime FineFirePrecedente;

            //if (e.PeriodTime.Days <= 28)
            //{   // se il periodo non include mesi, è "esatto", ci si può fidare per 
            //    // "tornare indietro" di un periodo
            //    FineFire = ProssimoFire.Add(e.ActivationLenght);
            //    FirePrecedente = ProssimoFire.Subtract(e.PeriodTime);
            //    FineFirePrecedente = FirePrecedente.Add(e.ActivationLenght);
            //}
            //else
            //{ 
            // fire the event
            try
            {
                e.AlreadyFired = true;
                string target = e.URL ?.Trim();
                if (string.IsNullOrEmpty(target))
                {
                    MessageBox.Show("Manca l'URI od il percorso file ");
                    return;
                }

                // Se è un URI file://, ottieni il percorso locale
                if (Uri.TryCreate(target, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeFile)
                {
                    target = uri.LocalPath;
                }

                // lancia l'URL o il file con l'applicazione associata
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true, // fondamentale per aprire con l'app associata
                };
                System.Diagnostics.Process.Start(psi);
                this.Text = "LaunchPlanner (" + e.Description + ")";
                // NotifyIcon.Text non accetta testi oltre i 127 caratteri
                notifyIcon1.Text = this.Text.Length > 127 ? this.Text.Substring(0, 127) : this.Text;
                txtLog.Text += DateTime.Now + " " + e.Description + "\r\n";
            }
            catch
            {
                MessageBox.Show("L'URL indicato non è stato trovato\n" + e.URL);
            }
        }
        private DateTime nextTimeSameWeekDay(DayOfWeek Day, DateTime Now, Event Event)
        {
            DateTime nextEventInstant = new DateTime(Now.Year, Now.Month, Now.Day, Event.StartAll.Hour,
                                        Event.StartAll.Minute, Event.StartAll.Second);
            while (nextEventInstant.DayOfWeek != Day)
            {
                nextEventInstant = nextEventInstant.AddDays(1);
            }
            return nextEventInstant;
        }
        private void readFile(String nFile)
        {
            char tab = '\t';
            int nLine = 0;

            try
            {
                // lettura del file di testo che contiene le definizioni degli eventi
                // prova ad aprire il file
                Encoding fileEncoding = Encoding.UTF8;

                using FileStream fsIn = new FileStream(nFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                using StreamReader sr = new StreamReader(fsIn, fileEncoding, true);

                // header: thrown 
                String Line = sr.ReadLine();

                int i = 0;
                nLine = 2;
                // read the file's rows 
                for (Line = sr.ReadLine(); Line != null; Line = sr.ReadLine())
                //TODO block syntax errors (albeit not existing, if you just use the GUI configurator)
                {
                    if (!(Line.StartsWith("'"))) // ' stands for a comment
                    {
                        string[] Campi = Line.Split(tab);  // generate fields 

                        Event ev = new Event();

                        // Parsing robusto di StartAll e FinishAll usando formati italiani e accettando sia ':' sia '.' come separatori orari.
                        string rawStart = Campi[0].Trim();
                        string rawFinish = Campi[1].Trim();

                        string[] acceptedFormats = new[]
                        {
                            "dd/MM/yyyy HH.mm.ss",
                            "d/M/yyyy H.m.s",
                            "dd/MM/yyyy H.mm.ss",
                            "dd/MM/yyyy HH:mm:ss",
                            "d/M/yyyy H:m:s",
                            "dd/MM/yyyy H:m:s",
                            "dd/MM/yyyy",
                            "yyyy-MM-dd HH:mm:ss",
                            "yyyy-MM-ddTHH:mm:ss"
                        };

                        DateTime parsedDate;
                        CultureInfo it = CultureInfo.GetCultureInfo("it-IT");

                        bool okStart = DateTime.TryParseExact(rawStart, acceptedFormats, it, DateTimeStyles.AllowWhiteSpaces, out parsedDate);
                        if (!okStart)
                        {
                            // prova a sostituire i punti con i due punti nella parte oraria e riprovare
                            string altStart = rawStart.Replace('.', ':');
                            okStart = DateTime.TryParseExact(altStart, acceptedFormats, it, DateTimeStyles.AllowWhiteSpaces, out parsedDate);
                            if (!okStart)
                            {
                                // fallback generico nella cultura italiana
                                okStart = DateTime.TryParse(rawStart, it, DateTimeStyles.AllowWhiteSpaces, out parsedDate);
                            }
                        }

                        if (!okStart)
                        {
                            MessageBox.Show("Formato data non valido (inizio): " + rawStart + " (riga " + nLine + ")",
                                "Errore nel formato della data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            nLine++;
                            continue; // salta questa riga
                        }
                        ev.StartAll = parsedDate;

                        bool okFinish = DateTime.TryParseExact(rawFinish, acceptedFormats, it, DateTimeStyles.AllowWhiteSpaces, out parsedDate);
                        if (!okFinish)
                        {
                            string altFinish = rawFinish.Replace('.', ':');
                            okFinish = DateTime.TryParseExact(altFinish, acceptedFormats, it, DateTimeStyles.AllowWhiteSpaces, out parsedDate);
                            if (!okFinish)
                            {
                                okFinish = DateTime.TryParse(rawFinish, it, DateTimeStyles.AllowWhiteSpaces, out parsedDate);
                            }
                        }

                        if (!okFinish)
                        {
                            MessageBox.Show("Formato data non valido (fine): " + rawFinish + " (riga " + nLine + ")",
                                "Errore nel formato della data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            nLine++;
                            continue; // salta questa riga
                        }
                        ev.FinishAll = parsedDate;

                        string[] temp = Campi[2].Split(':');
                        if (temp[0].Contains("."))
                        {
                            // c'è il numero di giorni
                            int giorni = int.Parse(temp[0].Substring(0, temp[0].IndexOf(".")));
                            int ore = int.Parse(temp[0].Substring(temp[0].IndexOf(".") + 1));
                            ev.Duration = new TimeSpan(giorni, ore, Convert.ToInt32(temp[1]),
                                Convert.ToInt32(temp[2]));
                        }
                        else
                        {
                            ev.Duration = new TimeSpan(Convert.ToInt32(temp[0]), Convert.ToInt32(temp[1]),
                                Convert.ToInt32(temp[2]));
                        }
                        ev.PeriodType = getTypeOfPeriodFromString(Campi[3]);
                        ev.Description = Campi[4];
                        ev.URL = Campi[5];
                        if (ev.URL.IndexOf("http", 0) > 0)
                            ev.URL = ev.URL.Replace("./", Application.StartupPath + "/").Replace("\\", "/");
                        ev.AlreadyFired = false;
                        ev.Enabled = Convert.ToBoolean(Campi[6]);

                        eventi.Add(ev);

                        i++;
                    }
                    nLine++;
                }
                // chiusura dello StreamReader
                sr.Close(); // This also closes the FileStream (fsIn)
            }
            catch
            {	// il nome del file è sbagliato o non si riesce al leggerlo
                if (nFile != "")
                {
                    MessageBox.Show("Il file " + nFile + " non è leggibile, controllarne il contenuto (riga " + nLine + ")",
                        "Errore nel formato del file",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    // provo di nuovo ad aprirlo, questa volta per modificarlo
                    try
                    {
                        openWithAssociatedApp(nFile);
                    }
                    catch
                    {
                        //MessageBox.Show("Il file di configurazione non esiste: copia del file standard nella cartella dell'utente");
                        //System.IO.File.Copy(Application.StartupPath + nomeFileDati, pathEFileDati + nomeFileDati);
                        //MessageBox.Show("Modifica del nuovo file di configurazione dell'utente"); 
                        //System.Diagnostics.Process.Start("file://" + pathEFileDati + nomeFileDati); 
                    }
                }
            }
        }
        internal Event.Periodicity getTypeOfPeriodFromString(string stPeriodo)
        {
            // Gestione robusta: evita eccezioni se la stringa è nulla o più corta di 5 caratteri
            if (string.IsNullOrWhiteSpace(stPeriodo))
                return Event.Periodicity.NonPeriodical;

            string controllo = stPeriodo.Trim().ToLowerInvariant();

            // Non chiamare Substring(0,5) se la stringa è più corta di 5 caratteri
            if (controllo.Length > 5)
                controllo = controllo.Substring(0, 5);

            switch (controllo)
            {
                case "singl":
                    return Event.Periodicity.SingleShot;
                case "nessu":
                case "none":
                case "nonpe":
                    return Event.Periodicity.NonPeriodical;
                case "secon":
                    return Event.Periodicity.Second;
                case "minut":
                case "onemi":
                    return Event.Periodicity.OneMinute;
                case "quart":
                    return Event.Periodicity.QuarterOfHour;
                case "mezzo":
                case "halfh":
                    return Event.Periodicity.HalfHour;
                case "ora":
                case "hour":
                    return Event.Periodicity.Hour;
                case "giorn":
                case "day":
                    return Event.Periodicity.Day;
                case "feria":
                case "weekd":
                    return Event.Periodicity.Weekday;
                case "week":
                case "setti":
                case "weekl":
                    return Event.Periodicity.Weekly;
                case "fines":
                case "weeke":
                    return Event.Periodicity.Weekend;
                case "luned":
                case "monda":
                    return Event.Periodicity.Monday;
                case "marte":
                case "tuesd":
                    return Event.Periodicity.Tuesday;
                case "merco":
                case "wedne":
                    return Event.Periodicity.Wednesday;
                case "giove":
                case "thurs":
                    return Event.Periodicity.Thursday;
                case "vener":
                case "frida":
                    return Event.Periodicity.Friday;
                case "sabat":
                case "satur":
                    return Event.Periodicity.Saturday;
                case "domen":
                case "sunda":
                    return Event.Periodicity.Sunday;
                case "mese":
                case "month":
                    return Event.Periodicity.Month;
                case "anno":
                case "year":
                    return Event.Periodicity.Year;
                case "custo":
                case "perso":
                    return Event.Periodicity.Custom;
                default:
                    return Event.Periodicity.NonPeriodical;
            }
        }
        private TimeSpan period(string stPeriodo)
        {
            string controllo = stPeriodo.ToLower();
            if (controllo.Length > 3) controllo = controllo.Substring(0, 4);
            switch (controllo)
            {
                case "ness":
                    // nessuno
                    return new TimeSpan(0, 0, 0, 0);

                case "seco":
                    // secondo
                    return new TimeSpan(0, 0, 1);

                case "minu":
                    // minuto
                    return new TimeSpan(0, 1, 0);

                case "quar":
                    // quartodora
                    return new TimeSpan(0, 15, 0);

                case "mezz":
                    // mezzora
                    return new TimeSpan(0, 30, 0);

                case "ora":
                    // ora
                    return new TimeSpan(1, 0, 0);

                case "gior":
                // giorno
                case "feri":
                // feriali
                case "week":
                // weekend 
                case "fine":
                // finesettimana
                case "lune":
                // lunedì
                case "mart":
                // martedì 
                case "merc":
                // mercoledì
                case "giov":
                //giovedi
                case "vene":
                //venerdi
                case "saba":
                // sabato
                case "dome":
                    // domenica

                    // scatta tutti i giorni, in seguito, eventualmente, controllerà se i giorni vanno bene
                    return new TimeSpan(24, 0, 0);

                case "sett":
                    // settimana = 7 giorni
                    return new TimeSpan(24 * 7, 0, 0);

                case "mese":
                    // mese
                    // il periodo non è fisso perchè dipende dalla lunghezza del mese,
                    // QUESTO CALCOLO è SOLO APPROSSIMATIVO (considera ogni mese di 30 giorni)
                    return new TimeSpan(24 * 30, 0, 0);

                case "anno":
                    // anno
                    // il periodo non è fisso perchè dipende dalla lunghezza dell'anno,
                    // QUESTO CALCOLO è SOLO APPROSSIMATIVO (considera un anno di 365 giorni)
                    return new TimeSpan(24 * 365, 0, 0);

                default:
                    // prova a leggerlo come una data, per assegnarlo come stPeriodo "custom", se non ce la fa si lamenta
                    try
                    {
                        // cerca il prossimo inizio del stPeriodo e, se è il caso, spara l'evento
                        // parse della stringa che definisce il stPeriodo
                        string[] temp = stPeriodo.Split(' ');
                        string[] day = temp[0].Split('-');
                        string[] hour = temp[1].Split(':');

                        // il periodo non è fisso perchè dipende dalla lunghezza dell'anno,
                        // QUESTO CALCOLO è SOLO APPROSSIMATIVO (considera un anno di 365 giorni)
                        return new TimeSpan(24 * 365 * Convert.ToInt32(day[0]) + 24 * 30 * Convert.ToInt32(day[1])
                                            + 24 * Convert.ToInt32(day[2]) + Convert.ToInt32(hour[0]),
                                            Convert.ToInt32(hour[1]), Convert.ToInt32(hour[2]));
                    }
                    catch
                    {
                        MessageBox.Show("Errore nella definizione del periodo dell'evento");
                        return new TimeSpan();
                    }
            }
        }
        private void letturaFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            eventi.Clear();
            readFile(pathEFileDati);
        }
        private void modificaFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                openWithAssociatedApp(pathEFileDati);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossibile aprire il file " + pathEFileDati + "\n" + ex.Message);
            }
        }
        private static void openWithAssociatedApp(string nFile)
        {
            // Senza UseShellExecute .NET non apre i documenti con l'applicazione associata
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = nFile,
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(psi);
        }
        private void frmLaunchPlanner_Load(object sender, EventArgs e)
        {
            MinimizeInTray();
            readFile(pathEFileDati);
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gamon.frmAbout f = new gamon.frmAbout();
            f.Show();
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            gamon.LaunchPlanner.frmConfiguration frmConf =
                new gamon.LaunchPlanner.frmConfiguration(eventi, this);
            frmConf.Show();
        }
        public void ScriviFile(string nFile)
        {
            try
            {
                // scrittura del file di testo che contiene le definizioni degli eventi

                // prova ad aprire il file
                Encoding fileEncoding = Encoding.UTF8;
                using FileStream fsOut = new FileStream(nFile, FileMode.Create, FileAccess.Write);
                using StreamWriter sw = new StreamWriter(fsOut, fileEncoding);

                // prima riga di intestazione:
                string Line = "Istante Inizio\tIstante Fine\tDurata\tPeriodicità\tDescrizione\tURL\tAbilitato";
                sw.WriteLine(Line);

                for (int i = 0; i < eventi.Count; i++)
                {
                    Event ev = (Event)eventi[i];
                    Line = ev.StartAll + "\t" + ev.FinishAll + "\t" + ev.Duration + "\t" +
                            Enum.GetName(typeof(Event.Periodicity), ev.PeriodType) + "\t" + ev.Description + "\t" + ev.URL + "\t" +
                            ev.Enabled;
                    sw.WriteLine(Line);
                }
                // chiusura dello Stream
                sw.Close();
            }
            catch (Exception ex)
            {
                // se non si riesce a scrivere il file, avverte
                MessageBox.Show("Non è stato possibile scrivere il file " + nFile,
                    "Errore nella scrittura.\r\n" + ex.Message,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void Salva()
        {
            ScriviFile(pathEFileDati);
        }
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            gamon.frmAbout f = new gamon.frmAbout();
            f.Show();
        }
    }
}