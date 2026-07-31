using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using StudentAccountTSTU.Services;

using WebAccount.Interfaces;


namespace StudentAccountTSTU.ViewModels; 

public partial class MainViewModel : ViewModelBase {
    private readonly Settings _settings;
    private readonly IHttpService _httpService;
    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks = new();

    private ViewModelBase currentPage;
    private int currentPageIndex;

    private bool isAuthenticated = false;

    public MainViewModel() {
        _settings = App.Services.GetRequiredService<Settings>();
        _httpService = App.Services.GetRequiredService<IHttpService>();
        _webAccount = App.Services.GetRequiredService<WebAccount.WebAccount>();

        _settings.Save();

        currentPage = new LoginViewModel(this, _settings, _webAccount);
    }

    public ViewModelBase CurrentPage {
        get => currentPage;
        set {
            currentPage = value;
            OnPropertyChanged();
        }
    }

    public int CurrentPageIndex {
        get => currentPageIndex;
        set {
            if (currentPageIndex != value) {
                currentPageIndex = value;
                OnPropertyChanged();
                NavigateToPage(value);
            }
        }
    }

    public bool IsAuthenticated {
        get => isAuthenticated;
        set {
            isAuthenticated = value;
            OnPropertyChanged();
        }
    }

    public void Login() {
        IsAuthenticated = true;
        CurrentPage = new HomeViewModel();
        CurrentPageIndex = 0;
    }

    public void Logout() {
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
            2 => new MarksViewModel(_activeLoadingTasks, _webAccount),
            3 => new ScheduleViewModel(_activeLoadingTasks, _webAccount),
            4 => new ReportCardViewModel(_activeLoadingTasks, _webAccount),
            5 => new RatingViewModel(),
            6 => new SettingsViewModel(this, _settings, _webAccount),
            _ => CurrentPage
        };
    }
}
