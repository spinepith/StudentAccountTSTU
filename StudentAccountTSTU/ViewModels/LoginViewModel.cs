using System;
using System.Runtime;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Crypto;
using StudentAccountTSTU.Services;


namespace StudentAccountTSTU.ViewModels;

internal partial class LoginViewModel : ViewModelBase {
    private readonly MainViewModel _mainViewModel;
    private readonly Settings _settings;
    private readonly WebAccount.WebAccount _webAccount;

    private string username     = string.Empty;
    private string password     = string.Empty;
    private string errorMessage = string.Empty;
    private bool isLoading      = false;

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

    public string Username {
        get => username;
        set {
            username = value;
            OnPropertyChanged();
        }
    }

    public string Password {
        get => password;
        set {
            password = value;
            OnPropertyChanged();
        }
    }

    public string AuthMessage {
        get => errorMessage;
        set {
            errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading {
        get => isLoading;
        set {
            isLoading = value;
            OnPropertyChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task Login() {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            return;

        IsLoading = true;
        LoginCommand.NotifyCanExecuteChanged();

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
            LoginCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanLogin() => !IsLoading;
}
