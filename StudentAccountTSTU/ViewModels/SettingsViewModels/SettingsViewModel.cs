using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;


namespace StudentAccountTSTU.ViewModels.SettingsViewModels;

internal partial class SettingsViewModel : ViewModelBase {
    private readonly MainViewModel _mainViewModel;
    private readonly WebAccount.WebAccount _webAccount;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private Settings _settings;

    internal SettingsViewModel(MainViewModel mainViewModel, Settings settings, WebAccount.WebAccount webAccount) {
        _mainViewModel = mainViewModel;
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
    private async Task Logout() {
        if (await _webAccount.LogoutAsync()) {
            Settings.DeviceId     = null;
            Settings.UserLogin    = null;
            Settings.UserPassword = null;

            _mainViewModel.Logout();
        }
    }
}
