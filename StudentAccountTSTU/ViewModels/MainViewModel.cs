using Microsoft.Extensions.DependencyInjection;

using WebAccount.Interfaces;


namespace StudentAccountTSTU.ViewModels; 

public partial class MainViewModel : ViewModelBase {
    private readonly Services.Settings _settings;
    private readonly IHttpService _httpService;
    private readonly WebAccount.WebAccount _webAccount;
    
    private ViewModelBase currentPage;
    private int currentPageIndex;

    private bool isAuthenticated = false;

    public MainViewModel() {
        _settings = App.Services.GetRequiredService<Services.Settings>();
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
    }

    private void NavigateToPage(int index) {
        if (!IsAuthenticated)
            return;

        CurrentPage = index switch {
            0 => new HomeViewModel(),
            1 => new UserDataViewModel(this, _httpService, _webAccount),
            2 => new MarksViewModel(),
            3 => new ScheduleViewModel(),
            4 => new ReportCardViewModel(),
            5 => new RatingViewModel(),
            6 => new SettingsViewModel(this, _settings, _webAccount),
            _ => CurrentPage
        };
    }
}
