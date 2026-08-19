using System.Collections.Generic;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

using StudentAccountTSTU.Services;
using StudentAccountTSTU.Stores;
using StudentAccountTSTU.ViewModels.SettingsViewModels;

using WebAccount.Interfaces;


namespace StudentAccountTSTU.ViewModels;

internal partial class MainViewModel : ViewModelBase {
    private readonly Settings _settings;

    private readonly Dictionary<string, Task?> _activeLoadingTasks = new();
    private readonly IHttpService _httpService;
    private readonly WebAccount.WebAccount _webAccount;


    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private int _currentPageIndex;

    public Settings Settings => _settings;

    internal MainViewModel() {
        _settings = App.Services.GetRequiredService<Settings>();

        _httpService = App.Services.GetRequiredService<IHttpService>();
        _webAccount = App.Services.GetRequiredService<WebAccount.WebAccount>();
        App.Services.GetRequiredService<StudentProfileStore>();

        CurrentPage = new LoginViewModel(this, _settings, _webAccount);
    }

    [RelayCommand]
    private void OnTabClicked(int index) {
        CurrentPageIndex = index;
        NavigateToPage(index);
    }

    internal void Login() {
        IsAuthenticated = true;
        CurrentPageIndex = 0;
        NavigateToPage(CurrentPageIndex);
    }

    internal void Logout() {
        IsAuthenticated = false;
        CurrentPageIndex = 0;
        CurrentPage = new LoginViewModel(this, _settings, _webAccount);
        FileStorage.RemoveDirectory("Data");
    }

    private void NavigateToPage(int index) {
        if (!IsAuthenticated)
            return;

        CurrentPage = index switch {
            0 => new HomeViewModel(_activeLoadingTasks, _httpService, _webAccount, _settings),
            1 => new UserDataViewModel(_activeLoadingTasks, _httpService, _webAccount),
            2 => new LessonsViewModel(_activeLoadingTasks, _webAccount),
            3 => new ScheduleViewModel(_activeLoadingTasks, _webAccount, _settings),
            4 => new ReportCardViewModel(_activeLoadingTasks, _webAccount),
            5 => new GroupsViewModel(_activeLoadingTasks, _webAccount),
            6 => new SettingsViewModel(this, _settings, _webAccount),
            _ => CurrentPage
        };
    }
}
