using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gamon.LaunchPlanner
{
    public partial class frmConfiguration : Form
    {
        List<Event> listEvents;
        private frmBackground frmBackground;
        private Event currentEvent;
        private int currentEventListIndex; 

        bool loading;

        internal frmConfiguration(List<Event> Eventi, frmBackground FrmBackground)
        {
            InitializeComponent();

            listEvents = Eventi;
            frmBackground = FrmBackground;
        }

        private void frmConfiguration_Load(object sender, EventArgs e)
        {
            //  load event types
            EventType et = new EventType(Event.Periodicity.Custom, "personalizzato");
            cmbPeriods.Items.Add(et);
            // zero evento singolo, sparato una sola volta, non periodico
            et = new EventType(Event.Periodicity.SingleShot, "non periodico");
            cmbPeriods.Items.Add(et);
            // evento sparato ogni minuto
            // (oppure al primo minuto disponibile dopo la fine della sua 
            // durata, se la durata è superiore al minuto)
            et = new EventType(Event.Periodicity.OneMinute, "minuto");
            cmbPeriods.Items.Add(et);
            et = new EventType(Event.Periodicity.HalfHour, "mezzora");
            cmbPeriods.Items.Add(et);
            // ora      evento sparato ad ogni ora, 
            // al minuto ed al secondo indicato dall'istante di inizio dell'evento
            et = new EventType(Event.Periodicity.Hour, "ora");
            cmbPeriods.Items.Add(et);
            // giorno   evento sparato ogni giorno, all'ora, minuto e secondo 
            // indicato dall'istante di inizio dell'evento
            et = new EventType(Event.Periodicity.Day, "giorno");
            cmbPeriods.Items.Add(et);
            // feriali  evento sparato da Lunedì a Venerdì, all'ora, minuto 
            // e secondo indicato dall'istante di inizio dell'evento
            et = new EventType(Event.Periodicity.Weekday, "feriali");
            cmbPeriods.Items.Add(et);
            // weekend o finesettimana  Sabato e Domenica, all'istante di inizio
            et = new EventType(Event.Periodicity.Weekend, "weekend");
            cmbPeriods.Items.Add(et);
            // lunedì   evento sparato il Lunedì, all'istante di inizio
            et = new EventType(Event.Periodicity.Thursday, "giovedì");
            cmbPeriods.Items.Add(et);
            et = new EventType(Event.Periodicity.Friday, "venerdì");
            cmbPeriods.Items.Add(et);
            et = new EventType(Event.Periodicity.Saturday, "sabato");
            cmbPeriods.Items.Add(et);
            et = new EventType(Event.Periodicity.Sunday, "domenica");
            cmbPeriods.Items.Add(et);
            // mese     evento sparato ad ogni mese, nell'istante di inizio dell'evento
            et = new EventType(Event.Periodicity.Month, "mese");
            cmbPeriods.Items.Add(et);
            // anno     evento sparato ad ogni anno, nell'istante di inizio dell'evento
            et = new EventType(Event.Periodicity.Year, "anno");
            cmbPeriods.Items.Add(et);
            // evento con la periodicità indicata nei textbox 
            et = new EventType(Event.Periodicity.NonPeriodical, "nessuno");

            cmbPeriods.Items.Add(et);
            dgrdEvents.Columns[0].Name = "Inizio";
            dgrdEvents.Columns[1].Name = "Fine";
            dgrdEvents.Columns[2].Name = "Durata";
            dgrdEvents.Columns[3].Name = "Periodo";
            dgrdEvents.Columns[4].Name = "Descrizione";
            dgrdEvents.Columns[5].Name = "URI";

            dgrdEvents.Columns[0].HeaderText = "Inizio";
            dgrdEvents.Columns[1].HeaderText = "Fine";
            dgrdEvents.Columns[2].HeaderText = "Durata";
            dgrdEvents.Columns[3].HeaderText = "Periodo";
            dgrdEvents.Columns[4].HeaderText = "Descrizione";
            dgrdEvents.Columns[5].HeaderText = "URL o path e nome file";

            fillDatagrid();
        }

        private void fillDatagrid()
        {
            dgrdEvents.Rows.Clear();
            for (int i = 0; i < listEvents.Count; i++)
            {
                Event ev = (Event)listEvents[i];
                string[] riga = {ev.StartAll.ToString(), ev.FinishAll.ToString(), ev.Duration.ToString(),
                                 Enum.GetName(typeof(Event.Periodicity), ev.PeriodType), ev.Description,ev.URL, ev.Enabled.ToString()};
                dgrdEvents.Rows.Add(riga);
            }
        }

        private void fillTextBoxes(Event Event)
        {
            loading = true;

            dtpkStart.Value = Event.StartAll;
            dtpkEnd.Value = Event.FinishAll;

            txtDaysDuration.Text = Event.Duration.Days.ToString();
            txtHoursDuration.Text = Event.Duration.Hours.ToString();
            txtMinutesDuration.Text = Event.Duration.Minutes.ToString();
            txtSecondsDuration.Text = Event.Duration.Seconds.ToString();

            int i = 0; 
            foreach (object o in cmbPeriods.Items)
            {
                EventType et = (EventType)o;
                if (et.periodicity == currentEvent.PeriodType)
                {
                    cmbPeriods.SelectedItem = cmbPeriods.Items[i];
                    break; 
                }
                i++; 
            }
            txtDescription.Text = Event.Description;
            txtURI.Text = Event.URL;
            chkEnabled.Checked = Event.Enabled;

            loading = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            frmBackground.Salva();
        }

        private void txtDurata_TextChanged(object sender, EventArgs e)
        {
            if (!loading && currentEvent != null)
            {
                int giorni;
                int.TryParse(txtDaysDuration.Text, out giorni); 
                int ore;
                int.TryParse(txtHoursDuration.Text, out ore);
                ore += giorni * 24; 
                int minuti; 
                int.TryParse(txtMinutesDuration.Text, out minuti);
                int secondi;
                int.TryParse(txtSecondsDuration.Text, out secondi);
                TimeSpan dummy = new TimeSpan(ore, minuti, secondi);
                currentEvent.Duration = dummy;

                dgrdEvents.Rows[currentEventListIndex].Cells[2].Value = dummy.ToString();
            }
        }

        private void dtpkInizio_ValueChanged(object sender, EventArgs e)
        {
            if (currentEvent != null)
            { 
                currentEvent.StartAll = dtpkStart.Value;
                dgrdEvents.Rows[currentEventListIndex].Cells[0].Value = dtpkStart.Value.ToString();
            }
        }

        private void dtpkFine_ValueChanged(object sender, EventArgs e)
        {
            if (currentEvent != null)
            {
                currentEvent.FinishAll = dtpkEnd.Value;
                dgrdEvents.Rows[currentEventListIndex].Cells[1].Value = dtpkEnd.Value.ToString();
            }
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            if (currentEvent != null)
            {
                currentEvent.Description = txtDescription.Text;
                dgrdEvents.Rows[currentEventListIndex].Cells[4].Value = txtDescription.Text;
            }
        }

            private void txtURI_TextChanged(object sender, EventArgs e)
        {
            if (currentEvent != null)
            {
                currentEvent.URL = txtURI.Text;
                dgrdEvents.Rows[currentEventListIndex].Cells[5].Value = txtURI.Text;
            }
        }

        private void cmbPeriodi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentEvent != null)
            {
                currentEvent.PeriodType = frmBackground.getTypeOfPeriodFromString(cmbPeriods.SelectedItem.ToString());
                switch (cmbPeriods.SelectedItem)
                {
                    case "custom":
                    case "personalizzato": // ?????
                        {
                            // evento singolo, sparato una sola volta, non periodico
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "zero":
                    case "nessuno": // ?????
                        {
                            // evento singolo, sparato una sola volta, non periodico
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "minuto":
                        {
                            // minuto   evento sparato ogni minuto 
                            // (oppure al primo minuto disponibile dopo la fine della sua 
                            // durata, se la durata è superiore al minuto)
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "1";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "quartodora":
                        {
                            // evento singolo, sparato una sola volta, non periodico
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "15";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "mezzora":
                        {
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "30";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "ora":
                        {
                            // ora: evento sparato ad ogni ora, 
                            // al minuto ed al secondo indicato dall'istante di inizio dell'evento
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "1";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "giorno":
                        {
                            // giorno: evento sparato ogni giorno, all'ora, minuto e secondo 
                            // indicato dall'istante di inizio dell'evento
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "1";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "feriali":
                        {
                            // feriali  evento sparato da Lunedì a Venerdì, all'ora, minuto 
                            // e secondo indicato dall'istante di inizio dell'evento
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "weekend":
                    case "finesettimana":
                        {
                            // weekend o finesettimana  Sabato e Domenica, all'istante di iniziO
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "lunedì":
                        {
                            // lunedì   evento sparato il Lunedì, all'istante di inizio
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "martedì":
                        {
                            // martedì evento sparato il Lunedì, all'istante di inizio
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "mercoledì":
                        {
                            // mercoledì vedi sopra 
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "giovedì":
                        {
                            // giovedì
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "venerdì":
                        {
                            // venerdì
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "sabato":
                        {
                            // sabato
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "domenica":
                        {
                            // domenica 
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "mese":
                        {
                            // mese     evento sparato ad ogni mese, nell'istante di inizio dell'evento
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    case "anno":
                        {
                            // anno     evento sparato ad ogni anno, nell'istante di inizio dell'evento
                            txtDaysPeriod.Enabled = false;
                            txtHoursPeriod.Enabled = false;
                            txtMinutesPeriod.Enabled = false;
                            txtSecondsPeriod.Enabled = false;
                            txtDaysPeriod.Text = "0";
                            txtHoursPeriod.Text = "0";
                            txtMinutesPeriod.Text = "0";
                            txtSecondsPeriod.Text = "0";
                            break;
                        }
                    default:
                        {
                            break; 
                        }
  
                }
                //currentEvent.Periodicità = cmbPeriodi.SelectedItem.ToString(); 
                dgrdEvents.Rows[currentEventListIndex].Cells[3].Value = currentEvent.PeriodType.ToString();
            }
        }

        private TimeSpan GetPeriodTime(object perodicity)
        {
            throw new NotImplementedException();
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (currentEvent != null)
            {
                currentEvent.Enabled = chkEnabled.Checked;
                dgrdEvents.Rows[currentEventListIndex].Cells[6].Value = chkEnabled.Checked;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnErase_Click(object sender, EventArgs e)
        {
            listEvents.Remove(currentEvent);
            fillDatagrid(); 
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Event eventoNuovo = new Event(); 
            eventoNuovo.Enabled = chkEnabled.Checked;
            try
            {
                eventoNuovo.PeriodType = frmBackground.getTypeOfPeriodFromString(cmbPeriods.SelectedItem.ToString());
            } catch {
                MessageBox.Show("Definire la periodicità dell'evento");
                return;
            }
            eventoNuovo.URL = txtURI.Text;
            eventoNuovo.Description = txtDescription.Text;
            eventoNuovo.StartAll = dtpkStart.Value;
            eventoNuovo.FinishAll = dtpkEnd.Value;
            
            int ore = int.Parse(txtDaysDuration.Text) * 24 + int.Parse(txtHoursDuration.Text);
            int minuti = int.Parse(txtMinutesDuration.Text);
            int secondi = int.Parse(txtSecondsDuration.Text);
            TimeSpan dummy = new TimeSpan(ore, minuti, secondi);
            eventoNuovo.Duration = dummy;
            listEvents.Add(eventoNuovo);
            fillDatagrid();
            dgrdEvents.ClearSelection();
            dgrdEvents.Rows[dgrdEvents.Rows.Count - 2].Selected = true; 
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            // spara l'evento
            try
            {
                System.Diagnostics.Process.Start(txtURI.Text);
            }
            catch
            {
                MessageBox.Show("L'URI indicato non è stato trovato\n(" + txtURI.Text + ") ");
            }
        }

        private void dgrdEvents_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgrdEvents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                currentEvent = (Event)listEvents[e.RowIndex];
                currentEventListIndex = e.RowIndex;
                fillTextBoxes(currentEvent);
            }
        }
    }
}
