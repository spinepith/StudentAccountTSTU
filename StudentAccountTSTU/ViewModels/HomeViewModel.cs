using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

using WebAccount.Interfaces;


namespace StudentAccountTSTU.ViewModels;

internal partial class HomeViewModel : ViewModelBase {
    private readonly Dictionary<string, Task?> _activeLoadingTasks;
    private readonly IHttpService _httpService;
    private readonly WebAccount.WebAccount _webAccount;
    private readonly Settings _settings;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private UserDataViewModel? _userDataViewModel;

    [ObservableProperty]
    private ScheduleViewModel? _scheduleViewModel;

    [ObservableProperty]
    private ReportCardViewModel? _reportCardViewModel;

    [ObservableProperty]
    private LessonsViewModel? _lessonsViewModel;

    [ObservableProperty]
    private GroupsViewModel? _groupsViewModel;

    [ObservableProperty]
    private Bitmap? _displayImage;

    [ObservableProperty]
    private RatingViewModel? _ratingViewModel;

    [ObservableProperty]
    private string? _selectedGroupName;

    [ObservableProperty]
    private MarksViewModel? _marksViewModel;

    internal HomeViewModel(Dictionary<string, Task?> activeLoadingTasks, IHttpService httpService, WebAccount.WebAccount webAccount, Settings settings) {
        _activeLoadingTasks = activeLoadingTasks;
        _httpService = httpService;
        _webAccount = webAccount;
        _settings = settings;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync() {
        IsLoading = true;

        var hasData = HasAnyData();

        UserDataViewModel = new UserDataViewModel(_activeLoadingTasks, _httpService, _webAccount);
        ScheduleViewModel = new ScheduleViewModel(_activeLoadingTasks, _webAccount, _settings);
        ReportCardViewModel = new ReportCardViewModel(_activeLoadingTasks, _webAccount);
        LessonsViewModel = new LessonsViewModel(_activeLoadingTasks, _webAccount);
        GroupsViewModel = new GroupsViewModel(_activeLoadingTasks, _webAccount);
        MarksViewModel = new MarksViewModel(_activeLoadingTasks, _webAccount, "Все", null!);

        _ = MonitorImageLoading();

        if (!hasData) {
            while (true) {
                bool anyCompleted = false;
                foreach (var kvp in _activeLoadingTasks) {
                    if (kvp.Value is null || kvp.Value.IsCompleted) {
                        anyCompleted = true;
                        break;
                    }
                }

                if (anyCompleted)
                    break;

                await Task.Delay(100);
            }
        }

        IsLoading = false;
    }

    private async Task MonitorImageLoading() {
        while (DisplayImage is null) {
            var avatarPath = Path.Combine("Data", "Avatar.jpg");
            if (FileStorage.CheckExists(avatarPath)) {
                try {
                    DisplayImage = new Bitmap(FileStorage.GetFullPath(avatarPath));
                    break;
                }
                catch { }
            }
            else if (UserDataViewModel?.UserImage is not null) {
                DisplayImage = UserDataViewModel.UserImage;
                break;
            }

            await Task.Delay(100);
        }
    }

    private bool HasAnyData() {
        var dataPath = FileStorage.GetFullPath("Data");

        if (!Directory.Exists(dataPath))
            return false;

        var excludedFiles = new[] {
            "Avatar.jpg",
            "ScheduleFirstType.jpg",
            "ScheduleSecondType1.jpg",
            "ScheduleSecondType2.jpg"
        };

        var allFiles = Directory.GetFiles(dataPath, "*", SearchOption.AllDirectories);
        foreach (var file in allFiles) {
            var fileName = Path.GetFileName(file);
            bool isExcluded = false;
            foreach (var excluded in excludedFiles) {
                if (fileName == excluded) {
                    isExcluded = true;
                    break;
                }
            }
            if (!isExcluded)
                return true;
        }
        return false;
    }

    [RelayCommand]
    private void SelectGroup(string groupName) {
        SelectedGroupName = groupName;
        RatingViewModel = new RatingViewModel(_activeLoadingTasks, _webAccount, groupName, null!);
    }
}
