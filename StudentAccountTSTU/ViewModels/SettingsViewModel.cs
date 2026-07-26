using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;


namespace StudentAccountTSTU.ViewModels;

internal partial class SettingsViewModel : ViewModelBase {
    private readonly MainViewModel _mainViewModel;
    private readonly Settings _settings;
    private readonly WebAccount.WebAccount _webAccount;

    public SettingsViewModel(MainViewModel mainViewModel, Settings settings, WebAccount.WebAccount webAccount) {
        _mainViewModel = mainViewModel;
        _settings = settings;
        _webAccount = webAccount;
    }

    [RelayCommand]
    private async Task Logout() {
        if (await _webAccount.LogoutAsync()) {
            _settings.DeviceId     = null;
            _settings.UserLogin    = null;
            _settings.UserPassword = null;
            _settings.Save();

            _mainViewModel.Logout();
        }
        else {

        }
    }
}
