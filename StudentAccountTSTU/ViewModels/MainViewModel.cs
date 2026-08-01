using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using StudentAccountTSTU.Stores;
using StudentAccountTSTU.Services;

using WebAccount.Interfaces;


namespace StudentAccountTSTU.ViewModels;

internal partial class MainViewModel : ViewModelBase {
    private readonly Settings _settings;
    private readonly StudentProfileStore _studentProfileStore;
    private readonly Dictionary<string, Task?> _activeLoadingTasks = new();

    private readonly IHttpService _httpService;
    private readonly WebAccount.WebAccount _webAccount;

    private ViewModelBase currentPage;
    private int currentPageIndex;

    private bool isAuthenticated = false;

    internal MainViewModel() {
        _settings = App.Services.GetRequiredService<Settings>();
        _studentProfileStore = App.Services.GetRequiredService<StudentProfileStore>();

        _httpService = App.Services.GetRequiredService<IHttpService>();
        _webAccount = App.Services.GetRequiredService<WebAccount.WebAccount>();

        _settings.Save();

        currentPage = new LoginViewModel(this, _settings, _webAccount);
    }

    internal ViewModelBase CurrentPage {
        get => currentPage;
        set {
            currentPage = value;
            OnPropertyChanged();
        }
    }

    internal int CurrentPageIndex {
        get => currentPageIndex;
        set {
            if (currentPageIndex != value) {
                currentPageIndex = value;
                OnPropertyChanged();
                NavigateToPage(value);
            }
        }
    }

    internal bool IsAuthenticated {
        get => isAuthenticated;
        set {
            isAuthenticated = value;
            OnPropertyChanged();
        }
    }

    internal void Login() {
        IsAuthenticated = true;
        CurrentPage = new HomeViewModel();
        CurrentPageIndex = 0;
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
            0 => new HomeViewModel(),
            1 => new UserDataViewModel(_activeLoadingTasks, _httpService, _webAccount),
            2 => new LessonsViewModel(_activeLoadingTasks, _webAccount),
            3 => new ScheduleViewModel(_activeLoadingTasks, _webAccount),
            4 => new ReportCardViewModel(_activeLoadingTasks, _webAccount),
            5 => new GroupsViewModel(_activeLoadingTasks, _webAccount),
            6 => new SettingsViewModel(this, _settings, _webAccount),
            _ => CurrentPage
        };
    }
}
