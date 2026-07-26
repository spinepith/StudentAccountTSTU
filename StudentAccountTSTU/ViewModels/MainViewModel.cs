using System.ComponentModel;
using System.Runtime.CompilerServices;

using CommunityToolkit.Mvvm.ComponentModel;


namespace StudentAccountTSTU.ViewModels; 

public partial class MainViewModel : ViewModelBase {
    private ViewModelBase currentPage;
    private bool isAuthenticated = false;
    private LoginViewModel loginViewModel;

    public MainViewModel() {
        loginViewModel = new LoginViewModel(this);
        currentPage = loginViewModel;
    }

    public ViewModelBase CurrentPage {
        get => currentPage;
        set {
            currentPage = value;
            OnPropertyChanged();
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
    }

    public void Logout() {
        IsAuthenticated = false;
        CurrentPage = loginViewModel;
    }

    public void OpenHome()       => CurrentPage = new HomeViewModel();
    public void OpenUserData()   => CurrentPage = new UserDataViewModel();
    public void OpenMarks()      => CurrentPage = new MarksViewModel();
    public void OpenSchedule()   => CurrentPage = new ScheduleViewModel();
    public void OpenReportCard() => CurrentPage = new ReportCardViewModel();
    public void OpenRating()     => CurrentPage = new RatingViewModel();
    public void OpenSettings()   => CurrentPage = new SettingsViewModel();
}
