using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;


namespace StudentAccountTSTU.ViewModels.SettingsViewModels;

internal partial class SettingsViewModel : ViewModelBase {
    private readonly HomeViewModel _parentViewModel;
    private readonly WebAccount.WebAccount _webAccount;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private Settings _settings;

    [ObservableProperty]
    private bool _removeDataButtonVisible = false;

    internal SettingsViewModel(Settings settings, WebAccount.WebAccount webAccount, HomeViewModel parentViewModel) {
        _parentViewModel = parentViewModel;
        _webAccount = webAccount;

        Settings = settings;
    }

    [RelayCommand]
    private void SelectSettings(string settingsPage) {
        CurrentPage = settingsPage switch {
            SettingsPages.CustomSchedule   => new CustomScheduleViewModel(this, Settings),
            SettingsPages.LoginAndPassword => new LoginAndPasswordViewModel(this, Settings),
            SettingsPages.AccentColor      => new AccentColorViewModel(this, Settings),
            SettingsPages.CustomAvatar     => new CustomAvatarViewModel(this, Settings),
            _ => CurrentPage
        };
    }

    [RelayCommand]
    private void ToggleSaveData() => Settings.SaveData = !Settings.SaveData;

    [RelayCommand]
    private void ToggleUseBlur() => Settings.UseBlur = !Settings.UseBlur;

    internal void BackToAllSettings() => CurrentPage = null;

    [RelayCommand]
    private void ShowRemoveSavedDataButton() {
        RemoveDataButtonVisible = !RemoveDataButtonVisible;
    }

    [RelayCommand]
    private void RemoveSavedData() {
        FileStorage.RemoveDirectory("Data");
        RemoveDataButtonVisible = false;
    }

    [RelayCommand]
    private async Task Logout() {
        if (await _webAccount.LogoutAsync()) {
            Settings.DeviceId     = null;
            Settings.UserLogin    = null;
            Settings.UserPassword = null;

            if (Avalonia.Application.Current?.DataContext is MainViewModel mainViewModel)
                mainViewModel.Logout();
        }
    }

    [RelayCommand]
    private void BackToHome() {
        _parentViewModel.BackToHome();
    }
}
