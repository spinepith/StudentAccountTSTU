using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Crypto;
using StudentAccountTSTU.Services;

namespace StudentAccountTSTU.ViewModels.SettingsViewModels; 

internal partial class LoginAndPasswordViewModel : ViewModelBase {
    private readonly SettingsViewModel _parentViewModel;
    private bool _hidden = true;

    [ObservableProperty]
    private Settings _settings;

    [ObservableProperty]
    private string _showHideText;

    [ObservableProperty]
    private string _login;

    [ObservableProperty]
    private string _password;

    internal LoginAndPasswordViewModel(SettingsViewModel parentViewModel, Settings settings) {
        _parentViewModel = parentViewModel;
        
        Settings = settings;
        ShowHideText = "ПОКАЗАТЬ";
        Login = "********";
        Password = "********";
    }

    [RelayCommand]
    private void ShowHide() {
        if (_hidden) {
            try {
                Login = CryptoService.Decrypt(Settings.UserLogin!, Settings.DeviceId!);
                Password = CryptoService.Decrypt(Settings.UserPassword!, Settings.DeviceId!);
            }
            catch {
                Login = "ЛОГИН";
                Password = "ПАРОЛЬ";
            }

            _hidden = false;
            ShowHideText = "СКРЫТЬ";
        }
        else {
            ShowHideText = "ПОКАЗАТЬ";
            Login = "********";
            Password = "********";
            _hidden = true;
        }
    }

    [RelayCommand]
    private void BackToAllSettings() {
        _parentViewModel.BackToAllSettings();
    }
}
