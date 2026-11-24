# Meccanismo di background di LaunchPlanner

LaunchPlanner deve lanciare un URL o un file all'ora prevista, anche quando l'utente non sta guardando il programma. Le tre piattaforme del progetto MAUI offrono strumenti molto diversi per farlo, quindi il meccanismo è implementato tre volte dietro la stessa interfaccia.

## Parte comune (LaunchPlanner.Core)

| Elemento | Ruolo |
|---|---|
| `IBackgroundScheduler` (`Start`, `Rearm`) | Contratto della parte residente, in `Services/PlatformContracts.cs` |
| `EventScheduler` | Calcola cosa è in scadenza (`TakeDue`), la prossima sveglia (`NextWake`) e le prossime occorrenze (`Upcoming`) |
| `FiringService` | Lancia gli eventi in scadenza e tiene il registro di ciò che è già partito (`FiredLog`) |
| `IEventLauncher` | Apre l'URL o il file con ciò che offre la piattaforma |
| `IStartupSettings` | Opzione di avvio automatico del tab Configurazione |

Il flusso è sempre lo stesso: qualcuno chiama `FiringService.RunDueAsync(adesso)`, che chiede a `EventScheduler.TakeDue` gli eventi scaduti, li segna come lanciati, li salva nel registro e li passa a `IEventLauncher`. Il registro evita i doppi lanci: un evento già partito non riparte finché dura la sua finestra.

`Rearm()` va chiamato dopo ogni modifica salvata dell'elenco eventi e dopo ogni lancio. Su Windows non serve a nulla, su Android e Apple ricalcola la prossima sveglia.

Le tre implementazioni di `PlatformBackgroundScheduler` stanno nello stesso namespace `LaunchPlanner.Services`, in cartelle diverse. Ne compila una sola per piattaforma, e `MauiProgram` registra quella disponibile.

| Piattaforma | File |
|---|---|
| Windows | `Platforms/Windows/PlatformBackgroundScheduler.cs` |
| Android | `Platforms/Android/PlatformBackgroundScheduler.cs`, `EventAlarmReceiver.cs` |
| iOS / Mac Catalyst | `Services/Apple/PlatformBackgroundScheduler.cs`, `NotificationDelegate.cs`, `EventNotification.cs` |

## Windows: il programma resta vivo nel tray

Windows permette a un programma desktop di restare in esecuzione, quindi il meccanismo è il più semplice.

- **Controllo periodico.** `PlatformBackgroundScheduler` crea un `IDispatcherTimer` con intervallo di 1 secondo. A ogni tick chiama `FiringService.RunDueAsync(DateTime.Now)`. Poiché il timer rilegge ogni volta l'elenco, `Rearm()` è vuoto.
- **Avvio del timer.** `Start()` è idempotente: crea il timer solo la prima volta. `App.OnStart` e `App.OnResume` lo chiamano.
- **Il programma non si chiude.** La finestra MAUI esiste sempre. `WindowHost` intercetta `AppWindow.Closing`, annulla la chiusura e nasconde la finestra. Il programma continua a vivere con l'icona nel tray (`TrayIcon`, basata su `NotifyIcon` di Windows Forms).
- **Uscita.** Dal tray o dal pulsante Uscita, `WindowHost.Exit()` imposta `isExiting` e chiama `Application.Quit()`. Solo allora il timer si ferma.
- **Lancio.** `PlatformEventLauncher` usa `Process.Start` con `UseShellExecute = true`, che apre URL e documenti con l'applicazione associata.
- **Avvio al login.** Il programma non è impacchettato (`WindowsPackageType=None`). L'opzione in Configurazione scrive la voce `LaunchPlanner` in `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, con il percorso dell'exe e l'argomento `--startup`. Con quell'argomento `WindowHost` nasconde la finestra dopo 1 secondo invece di 4.

Limiti: se il computer è in sospensione i tick si fermano. Al risveglio il primo tick lancia ciò che è scaduto, entro la finestra dell'evento.

## Android: AlarmManager, nessun servizio residente

Android non lascia un'app in esecuzione continua. Non c'è un foreground service: la parte residente è una sveglia di sistema.

### Armare la sveglia

`Rearm()` cancella la sveglia precedente e calcola con `EventScheduler.NextWake` l'istante del prossimo evento abilitato. Poi imposta un `PendingIntent` di tipo broadcast verso `EventAlarmReceiver`:

- con il permesso `SCHEDULE_EXACT_ALARM` usa `SetExactAndAllowWhileIdle`;
- senza il permesso, o prima di Android 12, usa `SetAndAllowWhileIdle`, che può arrivare in ritardo di qualche minuto;
- il tipo è `RtcWakeup`, quindi sveglia il dispositivo.

Se non ci sono eventi futuri, non resta nessuna sveglia.

### Quando suona

Android conserva la sveglia anche a processo terminato. Al suo scatto avvia il processo e chiama `EventAlarmReceiver.OnReceive`, che:

1. chiama `GoAsync()` per tenere vivo il receiver oltre il ritorno di `OnReceive`;
2. prende lo scheduler da `IPlatformApplication.Current.Services`, perché senza activity non c'è altro modo di accedere ai servizi;
3. chiama `RunAsync(requestPermissions: false)`, che esegue `RunDueAsync` con una finestra di tolleranza di 15 minuti e poi `Rearm()`;
4. chiude con `pending.Finish()` nel `finally`.

La finestra di 15 minuti serve perché, senza sveglie esatte o in modalità Doze, il sistema può consegnare la sveglia in ritardo. Senza tolleranza l'evento andrebbe perso.

### Riarmo dopo riavvio e cambi di sistema

Le sveglie di AlarmManager non sopravvivono al riavvio, a un cambio d'orologio o a un cambio di fuso. Lo stesso receiver ascolta quindi:

- `BOOT_COMPLETED`
- `MY_PACKAGE_REPLACED`
- `TIME_SET` e `TIMEZONE_CHANGED`
- `ACTION_SCHEDULE_EXACT_ALARM_PERMISSION_STATE_CHANGED`

Dopo `BOOT_COMPLETED` il riarmo avviene solo se l'opzione "Riarma le sveglie al riavvio del telefono" del tab Configurazione è attiva (preferenza `rearmAtBoot`, accesa di default). Gli altri eventi riarmano sempre.

### Apertura dell'URL

Da Android 10 un'app in background non può aprire attività. `PlatformEventLauncher` distingue due casi:

- **App in primo piano:** avvia direttamente l'`Intent` di visualizzazione.
- **App in background:** pubblica sempre una notifica con full-screen intent, che garantisce il link anche se l'apertura diretta fallisce. Se l'utente ha concesso il permesso di sovrapposizione (`SYSTEM_ALERT_WINDOW`), tenta anche l'apertura diretta.

I permessi (notifiche, sveglie esatte, sovrapposizione) vengono chiesti una sola volta, da `RequestPermissionsOnceAsync`, quando l'app è in primo piano.

### Istanza dell'app

`MainActivity` ha `LaunchMode.SingleTop`. Premere l'icona non crea una seconda interfaccia:

- app in primo piano: non cambia nulla;
- app in background, processo vivo: Android riporta in primo piano l'activity esistente e `OnResume` chiama `Start()`;
- processo terminato: parte un processo nuovo, `MauiProgram.CreateMauiApp` ricostruisce i singleton e `OnStart` chiama `Start()`. Gli eventi tornano dal file.

`Start()` richiede i permessi, esegue `RunDueAsync` per recuperare ciò che è scaduto mentre l'app era chiusa e riarma la sveglia.

### Uscita

Il pulsante Uscita chiama `PlatformBackgroundScheduler.Stop()`: cancella la sveglia e imposta il flag `stoppedByUser`. Poi chiude l'attività e termina il processo. Finché il flag è attivo, il receiver ignora i riarmi di sistema: senza il flag, un riavvio o un cambio d'ora riattiverebbe ciò che l'utente ha fermato. Il flag si azzera quando l'utente riapre l'app (`Start()`).

## iOS e Mac Catalyst: notifiche locali programmate in anticipo

iOS non permette codice residente né di lanciare un URL da un'app chiusa. Non c'è modo di eseguire logica allo scatto dell'ora. Quindi `PlatformBackgroundScheduler` fa il contrario: programma in anticipo una notifica locale per ogni occorrenza futura, e il sistema le consegna da solo.

### Programmare le notifiche

`Rearm()`:

1. rimuove tutte le richieste pendenti con `RemoveAllPendingNotificationRequests`;
2. chiede a `EventScheduler.Upcoming` le prossime 64 occorrenze in ordine di tempo, perché il sistema accetta al massimo 64 notifiche locali in attesa;
3. per ciascuna crea una `UNNotificationRequest` con un `UNCalendarNotificationTrigger` non ripetuto, impostato sulla data e ora esatte.

Ogni notifica porta con sé l'evento (inizio, occorrenza, descrizione, URL) nel `UserInfo`, a cura di `EventNotification`. Serve perché, quando la notifica arriva, l'app può essere stata terminata e riavviata.

### Consegna

`NotificationDelegate`, impostato in `AppDelegate`, gestisce due casi:

- **App in primo piano:** `WillPresentNotification` lancia subito l'evento e non mostra la notifica (`UNNotificationPresentationOptions.None`).
- **App in background o chiusa:** il sistema mostra la notifica e l'evento parte quando l'utente la tocca (`DidReceiveNotificationResponse`).

In entrambi i casi legge l'evento dal `UserInfo` e chiama `FiringService.LaunchOccurrenceAsync`. Questo metodo controlla il registro dei lanci per non ripetere un'occorrenza già partita.

### Avvio dell'app

`Start()` chiede l'autorizzazione per le notifiche (alert e suono), esegue `RunDueAsync` per recuperare le occorrenze scadute e poi chiama `Rearm()`. Se l'autorizzazione è negata, registra un errore nel log.

### Differenza con le altre piattaforme

Su iOS l'evento non parte da solo: l'utente deve toccare la notifica, perché il sistema non consente all'app chiusa di aprire un link. Questo non si può aggirare. Le notifiche già programmate sopravvivono al riavvio del dispositivo, quindi in Configurazione non c'è nessuna opzione: il tab mostra che non ce ne sono.

## Confronto

| | Windows | Android | iOS / Mac Catalyst |
|---|---|---|---|
| Processo residente | Sì, nel tray | No | No |
| Chi fa scattare l'evento | Timer da 1 s | Sveglia di AlarmManager | Notifica locale programmata |
| Il lancio è automatico | Sì | Sì, con limiti in background | No, serve il tocco sulla notifica |
| Dopo il riavvio | Voce Run, se l'utente la attiva | Receiver, se l'utente lo attiva | Automatico, gestito dal sistema |
| Ritardi possibili | Pochi secondi | Fino a 15 minuti senza sveglie esatte | Nessuno sull'orario della notifica |
| `Rearm()` | Vuoto | Ricalcola la sveglia | Ripianifica fino a 64 notifiche |
| Uscita | Tray o pulsante | Pulsante: cancella la sveglia | Non prevista |
