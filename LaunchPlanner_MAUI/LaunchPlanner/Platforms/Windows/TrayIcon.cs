using System.ComponentModel;
using System.Reflection;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;
using LaunchPlanner.Core.ViewModels;
using Forms = System.Windows.Forms;

namespace LaunchPlanner.Services
{
    public sealed class TrayIcon(WindowHost host, ActivityLog log, LogViewModel commands, LanguageService language) : IDisposable
    {
        private const string IconResource = "LaunchPlanner.ico";
        private const int MaximumTipLength = 127;
        private const int BalloonMilliseconds = 5000;

        private Forms.NotifyIcon? notifyIcon;

        public void Show()
        {
            if (notifyIcon is not null)
                return;

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(IconResource)!;
            notifyIcon = new Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(stream),
                Text = ActivityLog.DefaultStatus,
                Visible = true,
                ContextMenuStrip = CreateMenu(),
            };
            notifyIcon.DoubleClick += (_, _) => ToggleWindow();
            log.PropertyChanged += OnLogChanged;
            log.ErrorReported += OnError;
            language.Changed += OnLanguageChanged;
        }

        public void Dispose()
        {
            log.PropertyChanged -= OnLogChanged;
            log.ErrorReported -= OnError;
            language.Changed -= OnLanguageChanged;
            notifyIcon?.Dispose();
            notifyIcon = null;
        }

        private Forms.ContextMenuStrip CreateMenu()
        {
            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add(Strings.Tray_EditEvents, null, (_, _) => host.Show(AppShell.EventsRoute));
            menu.Items.Add(Strings.Log_EditFile, null, (_, _) => commands.EditFileCommand.Execute(null));
            menu.Items.Add(Strings.Tray_RunEvents, null, (_, _) => commands.ReloadCommand.Execute(null));
            menu.Items.Add(Strings.Tab_About, null, (_, _) => host.Show(AppShell.AboutRoute));
            menu.Items.Add(Strings.Log_Exit, null, (_, _) => Exit());
            return menu;
        }

        private void OnLanguageChanged()
        {
            if (notifyIcon is null)
                return;

            var previous = notifyIcon.ContextMenuStrip;
            notifyIcon.ContextMenuStrip = CreateMenu();
            previous?.Dispose();
        }

        private void ToggleWindow()
        {
            if (host.IsVisible)
                host.Hide();
            else
                host.Show(AppShell.LogRoute);
        }

        private void Exit()
        {
            Dispose();
            host.Exit();
        }

        private void OnLogChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ActivityLog.Status) || notifyIcon is null)
                return;

            notifyIcon.Text = log.Status.Length > MaximumTipLength ? log.Status[..MaximumTipLength] : log.Status;
            if (Application.Current?.Windows.FirstOrDefault() is { } window)
                window.Title = log.Status;
        }

        private void OnError(string message)
            => notifyIcon?.ShowBalloonTip(BalloonMilliseconds, ActivityLog.DefaultStatus, message, Forms.ToolTipIcon.Warning);
    }
}
