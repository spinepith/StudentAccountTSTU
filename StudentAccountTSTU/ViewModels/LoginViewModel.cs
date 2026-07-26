using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;


namespace StudentAccountTSTU.ViewModels;

internal partial class LoginViewModel : ViewModelBase {
    private readonly MainViewModel mainViewModel;
    private readonly WebAccount.WebAccount webAccount;

    private string username     = string.Empty;
    private string password     = string.Empty;
    private string errorMessage = string.Empty;
    private bool isLoading      = false;

    public LoginViewModel(MainViewModel mainViewModel) {
        this.mainViewModel = mainViewModel;
        webAccount = App.Services.GetRequiredService<WebAccount.WebAccount>();
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

        var result = await webAccount.LoginAsync(Username, Password);

        if (result is null) {
            AuthMessage = "УСПЕШНО";
            mainViewModel.Login();
        }
        else {
            AuthMessage = result;
            Password = string.Empty;
            IsLoading = false;
            LoginCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanLogin() => !IsLoading;
}
