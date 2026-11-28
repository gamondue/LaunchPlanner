using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Resources;
using LaunchPlanner.Core.Services;

namespace LaunchPlanner.Core.ViewModels;

public sealed partial class SettingsViewModel(
    IStartupSettings startup,
    IDataFolderSettings dataFolder,
    LanguageService language,
    ActivityLog log) : ObservableObject
{
    public IReadOnlyList<LanguageOption> Languages => language.Options;

    // Il cambio ricostruisce l'interfaccia: la pagina che ospita il selettore viene sostituita
    public LanguageOption? SelectedLanguage
    {
        get => Languages.First(option => option.Code == language.Current);
        set
        {
            if (value is not null)
                language.Change(value.Code);
        }
    }

    public bool IsStartupSupported => startup.IsSupported;
    public string StartupCaption => startup.Caption;

    public bool IsDataFolderSupported => dataFolder.IsSupported;
    public string ChooseDataFolderCaption => dataFolder.ChooseCaption;

    public bool IsStartupEnabled
    {
        get => startup.IsEnabled;
        set => startup.IsEnabled = value;
    }

    public bool IsCloudFileSupported => dataFolder.IsCloudFileSupported;

    // Dove sta il file degli eventi: il nome del file nel cloud, altrimenti la cartella
    public string DataFolder => dataFolder.CloudFileName ?? dataFolder.SelectedFolder;

    public bool IsCustomDataFolder =>
        dataFolder.CloudFileName is not null || !IsSameFolder(dataFolder.SelectedFolder, dataFolder.DefaultFolder);

    // Si accende dopo una modifica, perché la nuova cartella vale dal prossimo avvio
    [ObservableProperty]
    private bool isRestartNeeded;

    [RelayCommand]
    private async Task ChooseDataFolderAsync()
    {
        string? picked;
        try
        {
            picked = await dataFolder.PickFolderAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            log.AddError(string.Format(Strings.Error_DataFolderChangeFailed, ex.Message));
            return;
        }

        if (picked is not null) ChangeDataFolder(picked);
    }

    [RelayCommand]
    private async Task ChooseCloudFileAsync()
    {
        try
        {
            if (!await dataFolder.PickCloudFileAsync()) return;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log.AddError(string.Format(Strings.Error_DataFolderChangeFailed, ex.Message));
            return;
        }

        OnDataLocationChanged();
    }

    [RelayCommand]
    private void UseDefaultDataFolder() => ChangeDataFolder(null);

    private void ChangeDataFolder(string? folder)
    {
        if (dataFolder.CloudFileName is null
            && IsSameFolder(folder ?? dataFolder.DefaultFolder, dataFolder.SelectedFolder)) return;

        try
        {
            dataFolder.Select(folder);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log.AddError(string.Format(Strings.Error_DataFolderChangeFailed, ex.Message));
            return;
        }

        OnDataLocationChanged();
    }

    private void OnDataLocationChanged()
    {
        IsRestartNeeded = true;
        OnPropertyChanged(nameof(DataFolder));
        OnPropertyChanged(nameof(IsCustomDataFolder));
    }

    private static bool IsSameFolder(string a, string b) =>
        string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
