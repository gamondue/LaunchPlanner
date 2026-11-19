# Conversione a MAUI

## Differenze volute da Forms
Dove il vecchio codice era sbagliato ho seguito il comportamento dichiarato nei suoi commenti:
- `QuarterOfHour` e `HalfHour` ora scattano: prima il calcolo dava sempre un istante futuro, quindi mai. Riguarda anche un tuo evento reale.
- Nel weekend un evento feriale non blocca più quelli che lo seguono nell'elenco.
- `Year` e `Weekly` ora funzionano, e i giorni dal lunedì al mercoledì non sono più mappati su venerdì.
- `SingleShot` e `nessuno` scattano una volta sola, anche dopo aver ricaricato il file.
- Un evento già lanciato non riparte se riavvii il programma durante la sua durata.
- Ogni evento ha almeno 1 minuto di margine anche con durata zero (15 minuti su Android, dove le sveglie possono ritardare).
- Gli errori appaiono come fumetto nel tray invece che come finestra bloccante.
- Il salvataggio crea una copia `.bak` del file, perché commenti e righe errate non vengono riscritti.

Il periodo `Custom` resta ricavato dalla durata, come in Forms, e non implementa il formato `anni-mesi-giorni` citato solo nei commenti.

## Limiti delle piattaforme
- **Android:** da Android 10 un'app in background non può aprire link da sola, quindi l'utente riceve una notifica e la tocca. L'app chiede il permesso di sovrapposizione per tentare anche l'apertura diretta, ma su Android 15 potrebbe non bastare. I percorsi di file locali in pratica non funzionano. La versione minima è passata ad Android 8.
- **iOS e Mac:** con app chiusa l'evento parte solo toccando la notifica. Se non la tocchi, l'evento non viene lanciato. Con periodi molto brevi le 64 notifiche finiscono presto, finché non riapri l'app.
- **Android con ritardi oltre i 15 minuti:** l'evento viene perso senza alcuna segnalazione.

## Localizzazione
I testi stanno in `LaunchPlanner.Core/Resources`: `Strings.resx` contiene l'inglese, usato anche per le lingue senza traduzione, e `Strings.it.resx` l'italiano. La lingua si sceglie nella pagina About e la scelta viene conservata: "Lingua di sistema" segue quella del sistema operativo, letta all'avvio del programma. Il cambio vale subito e ricostruisce le pagine ripartendo dall'About, senza riavviare e senza toccare gli eventi non ancora salvati.

Per aggiungere un testo si inserisce la stessa chiave in entrambi i file e la si usa con `Strings.Chiave` nel C# o con `{x:Static res:Strings.Chiave}` nella XAML. Per aggiungere una lingua si crea `Strings.<codice>.resx`, la si aggiunge a `LanguageService` e si aggiunge il codice a `CFBundleLocalizations` nei due `Info.plist`. I test in `LocalizationTests.cs` controllano solo il file italiano: una nuova lingua va aggiunta anche lì.

Il file degli eventi non si traduce: intestazione, periodicità e date restano uguali in ogni lingua, così resta compatibile con LaunchPlanner_Forms.

## Prima della pubblicazione
- L'`ApplicationId` è ancora `com.companyname.launchplanner`.
- L'icona deriva da un'immagine di soli 48×48 pixel, quindi risulterà sfocata.
