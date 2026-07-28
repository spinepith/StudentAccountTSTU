using System;
using System.Runtime;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Crypto;
using StudentAccountTSTU.Services;


namespace StudentAccountTSTU.ViewModels;

internal partial class LoginViewModel : ViewModelBase {
    private readonly MainViewModel _mainViewModel;
    private readonly Settings _settings;
    private readonly WebAccount.WebAccount _webAccount;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _authMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isLoading = false;

    public LoginViewModel(MainViewModel mainViewModel, Settings settings, WebAccount.WebAccount webAccount) {
        _mainViewModel = mainViewModel;
        _settings = settings;
        _webAccount = webAccount;

        if (_settings.DeviceId is not null && _settings.UserLogin is not null && _settings.UserPassword is not null) {
            Username = CryptoService.Decrypt(_settings.UserLogin, _settings.DeviceId);
            Password = CryptoService.Decrypt(_settings.UserPassword, _settings.DeviceId);
            _ = Login();
        }
        else {
            _settings.DeviceId     = null;
            _settings.UserLogin    = null;
            _settings.UserPassword = null;
            _settings.Save();
        }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task Login() {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            return;

        IsLoading = true;

        var result = await _webAccount.LoginAsync(Username, Password);
        if (result.succes is true) {
            AuthMessage = "УСПЕШНО";
            _mainViewModel.Login();

            _settings.BaseURL      = result.message;
            _settings.DeviceId     = Guid.NewGuid().ToString();
            _settings.UserLogin    = CryptoService.Encrypt(Username, _settings.DeviceId);
            _settings.UserPassword = CryptoService.Encrypt(Password, _settings.DeviceId);
            _settings.Save();
        }
        else {
            AuthMessage = result.message;
            Password = string.Empty;
            IsLoading = false;
        }
    }

    private bool CanLogin() => !IsLoading;
}
