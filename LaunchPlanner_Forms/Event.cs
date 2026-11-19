using System;

namespace gamon.LaunchPlanner
{
    class Event
    {
        public enum Periodicity
        {
            Custom,
            SingleShot,
            OneMinute,
            QuarterOfHour,
            HalfHour,
            Hour, 
            Day,
            Weekly,
            Weekday,
            Weekend,
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday,
            Month,
            Year,
            NonPeriodical,
            Second,
        }
        // Descrizione: stringa che descrive l'evento e che verrà mostrata dal programma. 
        //              METTERE DESCRIZIONI CORTE
        public string Description;
        // Inizio: istante di inizio dell'evento: prima di questo istante l'evento non viene mai "sparato"
        public DateTime StartAll;
        // DurataAttivazione: durata dell'evento a partire dall'istante di inizio. 
        // Se il programma parte durante DurataAttivazione l'evento viene sparato. 
        // Se DurataAttivazione = 0 ed il programma non controlla nell'istante dell'attivazione, l'evento non viene mai sparato!
        // METTERE SEMPRE ALMENO 5 SECONDI PER DurataAttivazione. 
        // Solo dopo DurataAttivazione l'evento è considerato finito (e può essere "risparato", se è periodico) 
        //TODO aggiustare in modo che funzioni così, ora l'evento che abbia una durata maggiore del suo periodo semplicemente non funziona
        public TimeSpan Duration;
        // FineTutto: istante di fine dell'evento. Dopo questo istante l'evento non "esiste" più, non viene sparato comunque, anche se periodico
        public DateTime FinishAll;
        // Periodicità: stringa che rappresenta il periodo di attivazione dell'evento
        public Periodicity PeriodType;
        public string LocalCaption; 
        public string URL;
        public bool Enabled; 
        // UltimaAttivazione: istante in cui l'evento è stato attivato per l'ultima volta
        public bool AlreadyFired;
    }
}
