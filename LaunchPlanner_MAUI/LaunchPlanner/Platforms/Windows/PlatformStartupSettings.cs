using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;
using Microsoft.Win32;

namespace LaunchPlanner.Services
{
    // Il programma non è impacchettato (WindowsPackageType=None): l'avvio al login
    // si ottiene con una voce nella chiave Run dell'utente corrente, senza diritti di amministratore
    public sealed class PlatformStartupSettings(ActivityLog log) : IStartupSettings
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "LaunchPlanner";

        public bool IsSupported => true;

        public string Caption => Strings.Settings_Startup_Windows;

        public bool IsEnabled
        {
            get
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
                    return key?.GetValue(ValueName) is not null;
                }
                catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
                {
                    log.AddError(string.Format(Strings.Error_StartupChangeFailed, ex.Message));
                    return false;
                }
            }
            set
            {
                try
                {
                    using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
                    if (value)
                        key.SetValue(ValueName, $"\"{Environment.ProcessPath}\" {WindowHost.StartupArgument}");
                    else
                        key.DeleteValue(ValueName, throwOnMissingValue: false);
                }
                catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
                {
                    log.AddError(string.Format(Strings.Error_StartupChangeFailed, ex.Message));
                }
            }
        }
    }
}
