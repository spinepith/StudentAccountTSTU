using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;
using StudentAccountTSTU.Services.Storage;
using StudentAccountTSTU.ViewModels.SettingsViewModels;

using WebAccount.Interfaces;


namespace StudentAccountTSTU.ViewModels;

internal partial class HomeViewModel : ViewModelBase {
    private readonly Dictionary<string, Task?> _activeLoadingTasks;
    private readonly Settings _settings;
    private readonly IHttpService _httpService;
    private readonly WebAccount.WebAccount _webAccount;

    [ObservableProperty]
    private ViewModelBase? _currentPage;


    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private Bitmap? _homeCustomImage;

    [ObservableProperty]
    private bool _showHomeCustomImage;

    [ObservableProperty]
    private bool _showHomeCustomImageWeek;

    [ObservableProperty]
    private string? _currentWeek;

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
    [NotifyCanExecuteChangedFor(nameof(SelectGroupCommand))]
    private RatingViewModel? _ratingViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelectGroupCommand))]
    private string? _selectedGroupName;

    [ObservableProperty]
    private MarksViewModel? _marksViewModel;

    private readonly MainViewModel _mainViewModel;

    internal HomeViewModel(Dictionary<string, Task?> activeLoadingTasks, IHttpService httpService, WebAccount.WebAccount webAccount, Settings settings, MainViewModel mainViewModel) {
        _activeLoadingTasks = activeLoadingTasks;
        _httpService = httpService;
        _webAccount = webAccount;
        _settings = settings;
        _mainViewModel = mainViewModel;

        _ = InitializeAsync();
    }

    [RelayCommand]
    private void OpenSettings() {
        CurrentPage = new SettingsViewModel(_settings, _webAccount, this);
    }

    internal async Task BackToHome(bool dataRemoved = false) {
        if (dataRemoved || !await HasAnyData()) {
            _mainViewModel.NavigateToPage(0);
            return;
        }

        CurrentPage = null;
        await UpdateHomeCustomImage();
    }

    private async Task InitializeAsync() {
        IsLoading = true;

        var hasData = await HasAnyData();

        UserDataViewModel = new UserDataViewModel(_activeLoadingTasks, _httpService, _webAccount);
        ScheduleViewModel = new ScheduleViewModel(_activeLoadingTasks, _webAccount, _settings);
        ScheduleViewModel.PropertyChanged += async (s, e) => {
            if (e.PropertyName is nameof(ScheduleViewModel.CurrentWeek))
                await UpdateHomeCustomImage();
        };
        await UpdateHomeCustomImage();
        ReportCardViewModel = new ReportCardViewModel(_activeLoadingTasks, _webAccount);
        LessonsViewModel = new LessonsViewModel(_activeLoadingTasks, _webAccount);
        GroupsViewModel = new GroupsViewModel(_activeLoadingTasks, _webAccount);
        GroupsViewModel.PropertyChanged += (s, e) => {
            if (e.PropertyName is nameof(GroupsViewModel.Groups))
                if (GroupsViewModel.Groups?.Count is 1 && SelectedGroupName is null)
                    SelectGroup(GroupsViewModel.Groups[0]);
        };
        if (GroupsViewModel.Groups?.Count is 1 && SelectedGroupName is null)
            SelectGroup(GroupsViewModel.Groups[0]);

        MarksViewModel = new MarksViewModel(_activeLoadingTasks, _webAccount, "Все", null!);

        _ = MonitorImageLoading();
        CheckForActiveRatingTask();

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

    private void CheckForActiveRatingTask() {
        foreach (var kvp in _activeLoadingTasks) {
            if (kvp.Value is not null && !kvp.Value.IsCompleted && kvp.Key.StartsWith("Rating_") && kvp.Key.EndsWith("_Init")) {
                var groupName = kvp.Key.Substring(7, kvp.Key.Length - 12);

                SelectedGroupName = groupName;
                RatingViewModel = new RatingViewModel(_activeLoadingTasks, _webAccount, groupName, null!);
                break;
            }
        }
    }

    private async Task MonitorImageLoading() {
        while (DisplayImage is null) {
            var customAvatarPath = Path.Combine("Data", "Avatar.jpg");
            var webAvatarPath = Path.Combine("Data", "UserImage.jpg");

            if (await FileStorage.CheckExistsAsync(customAvatarPath)) {
                try {
                    using var stream = await FileStorage.GetFileStreamAsync(customAvatarPath);
                    DisplayImage = new Bitmap(stream);
                    break;
                }
                catch { }
            }
            else if (await FileStorage.CheckExistsAsync(webAvatarPath)) {
                try {
                    using var stream = await FileStorage.GetFileStreamAsync(webAvatarPath);
                    DisplayImage = new Bitmap(stream);
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

    private async Task<bool> HasAnyData() {
        var allFiles = await FileStorage.GetFilesAsync("Data");

        if (allFiles is null || allFiles.Length is 0)
            return false;

        var excludedFiles = new[] {
            "Avatar.jpg",
            "ScheduleFirstType.jpg",
            "ScheduleSecondType1.jpg",
            "ScheduleSecondType2.jpg"
        };

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

    [RelayCommand(CanExecute = nameof(CanSelectGroup))]
    private void SelectGroup(string groupName) {
        SelectedGroupName = groupName;
        RatingViewModel = new RatingViewModel(_activeLoadingTasks, _webAccount, groupName, null!);
    }

    private bool CanSelectGroup(string groupName) {
        if (SelectedGroupName is not null)
            return false;

        foreach (var kvp in _activeLoadingTasks)
            if (kvp.Value is not null && !kvp.Value.IsCompleted && kvp.Key.StartsWith("Rating_"))
                return false;

        return true;
    }

    private async Task UpdateHomeCustomImage() {
        var firstTypePath = Path.Combine("Data", "ScheduleFirstType.jpg");
        var secondType1Path = Path.Combine("Data", "ScheduleSecondType1.jpg");
        var secondType2Path = Path.Combine("Data", "ScheduleSecondType2.jpg");

        var firstTypeExists = await FileStorage.CheckExistsAsync(firstTypePath);
        var secondType1Exists = await FileStorage.CheckExistsAsync(secondType1Path);
        var secondType2Exists = await FileStorage.CheckExistsAsync(secondType2Path);

        var hasSecondType = secondType1Exists || secondType2Exists;
        var hasFirstType = firstTypeExists;

        if (hasSecondType) {
            var currentWeek = ScheduleViewModel?.CurrentWeek;
            if (currentWeek is not null) {
                if (currentWeek is "НЕЧЕТНАЯ" && secondType1Exists) {
                    try {
                        using var stream = await FileStorage.GetFileStreamAsync(secondType1Path);
                        HomeCustomImage = new Bitmap(stream);
                        ShowHomeCustomImage = true;
                        ShowHomeCustomImageWeek = true;
                        CurrentWeek = currentWeek;
                    }
                    catch { }
                }
                else if (currentWeek is "ЧЕТНАЯ" && secondType2Exists) {
                    try {
                        using var stream = await FileStorage.GetFileStreamAsync(secondType2Path);
                        HomeCustomImage = new Bitmap(stream);
                        ShowHomeCustomImage = true;
                        ShowHomeCustomImageWeek = true;
                        CurrentWeek = currentWeek;
                    }
                    catch { }
                }
                else {
                    HomeCustomImage = null;
                    ShowHomeCustomImage = false;
                    ShowHomeCustomImageWeek = false;
                }
            }
            else {
                HomeCustomImage = null;
                ShowHomeCustomImage = false;
                ShowHomeCustomImageWeek = false;
            }
        }
        else if (hasFirstType) {
            try {
                using var stream = await FileStorage.GetFileStreamAsync(firstTypePath);
                HomeCustomImage = new Bitmap(stream);
                ShowHomeCustomImage = true;
                ShowHomeCustomImageWeek = false;
            }
            catch { }
        }
        else {
            HomeCustomImage = null;
            ShowHomeCustomImage = false;
            ShowHomeCustomImageWeek = false;
        }

        var customAvatarPath = Path.Combine("Data", "Avatar.jpg");
        var webAvatarPath = Path.Combine("Data", "UserImage.jpg");

        if (await FileStorage.CheckExistsAsync(customAvatarPath)) {
            try {
                using var stream = await FileStorage.GetFileStreamAsync(customAvatarPath);
                DisplayImage = new Bitmap(stream);
            }
            catch { }
        }
        else if (await FileStorage.CheckExistsAsync(webAvatarPath)) {
            try {
                using var stream = await FileStorage.GetFileStreamAsync(webAvatarPath);
                DisplayImage = new Bitmap(stream);
            }
            catch { }
        }
        else if (UserDataViewModel?.UserImage is not null)
            DisplayImage = UserDataViewModel.UserImage;
        else
            DisplayImage = null;
    }
}