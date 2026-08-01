using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

using WebAccount.Models;


namespace StudentAccountTSTU.ViewModels;

internal partial class StudentRatingWrapper : ObservableObject {
    [ObservableProperty]
    private string? _position;

    [ObservableProperty]
    private string? _fio;

    [ObservableProperty]
    private string? _rating;

    [ObservableProperty]
    private string? _group;

    [ObservableProperty]
    private bool _isCurrentUser;
}

internal partial class RatingTableWrapper : ObservableObject {
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private List<string>? _headers;

    [ObservableProperty]
    private List<StudentRatingWrapper>? _students;
}

internal partial class RatingViewModel : ViewModelBase {
    private readonly GroupsViewModel _parentViewModel;

    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks;
    private readonly string _groupName;

    [ObservableProperty]
    private RatingTableWrapper? _instituteRating;

    [ObservableProperty]
    private RatingTableWrapper? _groupRating;

    [ObservableProperty]
    private string? _currentGroup;

    [ObservableProperty]
    private string? _currentUserName;

    [ObservableProperty]
    private string? _lastUpdated;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand), nameof(BackToGroupsCommand))]
    private bool _isLoading;

    internal RatingViewModel(Dictionary<string, Task?> activeLoadingTasks, WebAccount.WebAccount webAccount, string groupName, GroupsViewModel parentViewModel) {
        _activeLoadingTasks = activeLoadingTasks;
        _webAccount = webAccount;
        _groupName = groupName;
        _parentViewModel = parentViewModel;

        CurrentGroup = _groupName;

        _ = InitializeDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, $"Rating_{CurrentGroup}_Refresh", GetRatingDataAsync(), LoadFromCacheAsync);
        IsLoading = false;
    }

    private async Task GetRatingDataAsync() {
        var ratingDirectory = Path.Combine("Data", "Ratings");
        InstituteRating = null;
        GroupRating = null;

        var ratingData = await _webAccount.GetRatingAsync(_groupName);
        if (ratingData is not null) {
            await FileStorage.SaveAsync(ratingData, Path.Combine(ratingDirectory, $"{_groupName}.json"));

            InstituteRating = ConvertToWrappedRating("Рейтинг института", ratingData.Headers, ratingData.Institute);
            GroupRating = ConvertToWrappedRating("Рейтинг группы", ratingData.Headers, ratingData.Group);

            UpdateLastModifiedDate(Path.Combine(ratingDirectory, $"{_groupName}.json"));
        }
    }

    private RatingTableWrapper? ConvertToWrappedRating(string name, List<string>? headers, List<Rating.Student>? students) {
        if (students is null || students.Count == 0)
            return null;

        return new RatingTableWrapper {
            Name = name,
            Headers = headers,
            Students = students.Select(
                student => new StudentRatingWrapper {
                    Position = student.Position,
                    Fio = student.Fio,
                    Rating = student.Rating,
                    Group = student.Group,
                    IsCurrentUser = string.Equals(student.Fio, CurrentUserName, System.StringComparison.OrdinalIgnoreCase)
                }
            ).ToList()
        };
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        IsLoading = true;
        CurrentUserName = await _webAccount.GetUserNameAsync();
        await InitializeWithCacheAsync(_activeLoadingTasks, $"Rating_{CurrentGroup}_Init", LoadRatingAsync(), LoadFromCacheAsync, $"Rating_{CurrentGroup}_Refresh");
        IsLoading = false;
    }

    private async Task LoadRatingAsync() {
        var path = Path.Combine("Data", "Ratings", $"{_groupName}.json");

        if (!FileStorage.CheckExists(path))
            await GetRatingDataAsync();
        else
            await LoadFromCacheAsync();
    }

    private async Task LoadFromCacheAsync() {
        var path = Path.Combine("Data", "Ratings", $"{_groupName}.json");

        if (FileStorage.CheckExists(path)) {
            var ratingData = await FileStorage.GetAsync<Rating>(path);
            if (ratingData is not null) {
                InstituteRating = ConvertToWrappedRating("Рейтинг института", ratingData.Headers, ratingData.Institute);
                GroupRating = ConvertToWrappedRating("Рейтинг группы", ratingData.Headers, ratingData.Group);
                UpdateLastModifiedDate(path);
            }
        }
    }

    private void UpdateLastModifiedDate(string filePath) {
        var lastModified = FileStorage.GetLastModified(filePath);
        if (lastModified.HasValue)
            LastUpdated = $"ОБНОВЛЕНО {lastModified.Value:dd.MM.yyyy HH:mm}";
        else
            LastUpdated = null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private void BackToGroups() {
        _parentViewModel.BackToGroups();
    }
}
