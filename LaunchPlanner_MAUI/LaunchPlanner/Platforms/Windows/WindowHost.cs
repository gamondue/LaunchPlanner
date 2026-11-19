using Microsoft.UI.Windowing;

namespace LaunchPlanner.Services
{
    // La finestra MAUI esiste sempre: il programma vive nel tray e la finestra viene solo nascosta
    public sealed class WindowHost
    {
        private static readonly TimeSpan SplashDuration = TimeSpan.FromSeconds(4);

        private Microsoft.UI.Xaml.Window? window;
        private bool isExiting;
        private bool isSplashing = true;

        public bool IsVisible => window?.AppWindow.IsVisible ?? false;

        public void Attach(Microsoft.UI.Xaml.Window nativeWindow)
        {
            window = nativeWindow;
            window.AppWindow.Closing += OnClosing;
            Application.Current!.Dispatcher.DispatchDelayed(SplashDuration, EndSplash);
        }

        public void Show(string route)
        {
            isSplashing = false;
            window?.AppWindow.Show();
            window?.Activate();
            _ = Shell.Current.GoToAsync(route);
        }

        public void Hide() => window?.AppWindow.Hide();

        public void Exit()
        {
            isExiting = true;
            Application.Current?.Quit();
        }

        // All'avvio si vede solo l'About, poi resta il tray
        private void EndSplash()
        {
            if (!isSplashing)
                return;

            isSplashing = false;
            Hide();
            _ = Shell.Current.GoToAsync(AppShell.LogRoute);
        }

        private void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if (isExiting)
                return;

            args.Cancel = true;
            Hide();
        }
    }
}
