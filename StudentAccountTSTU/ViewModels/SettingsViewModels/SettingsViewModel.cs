using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;
using StudentAccountTSTU.Services.Storage;


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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LogoutCommand))]
    private bool _isLoading;

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
            SettingsPages.Info             => new InfoViewModel(this),
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

    public bool DataRemoved { get; private set; } = false;

    [RelayCommand]
    private async Task RemoveSavedData() {
        await FileStorage.RemoveDirectoryAsync("Data");
        RemoveDataButtonVisible = false;
        DataRemoved = true;
    }

    [RelayCommand]
    private async Task Logout() {
        var mainViewModel = Avalonia.Application.Current?.DataContext as MainViewModel;
        
        if (mainViewModel is not null) {
            IsLoading = true;
            mainViewModel.IsAuthenticated = false;
        
            if (await _webAccount.LogoutAsync()) {
                Settings.DeviceId     = null;
                Settings.UserLogin    = null;
                Settings.UserPassword = null;
                await mainViewModel.Logout();
            }

            IsLoading = false;
        }
        
    }

    [RelayCommand]
    private void OpenInfo() {
        CurrentPage = new InfoViewModel(this);
    }

    [RelayCommand]
    private async Task BackToHome() {
        await _parentViewModel.BackToHome(DataRemoved);
    }
}
