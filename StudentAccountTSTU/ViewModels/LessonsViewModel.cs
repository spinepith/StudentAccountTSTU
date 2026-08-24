using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services.Storage;


namespace StudentAccountTSTU.ViewModels;

internal partial class LessonsViewModel : ViewModelBase {
    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private List<string>? _lessons;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _lastUpdated;

    internal LessonsViewModel(Dictionary<string, Task?> activeLoadingTasks, WebAccount.WebAccount webAccount) {
        _activeLoadingTasks = activeLoadingTasks;
        _webAccount = webAccount;

        _ = InitializeDataAsync();
    }

    [RelayCommand]
    private void SelectLesson(string lessonName) {
        CurrentPage = new MarksViewModel(_activeLoadingTasks, _webAccount, lessonName, this);
    }

    internal void BackToLessons() {
        CurrentPage = null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "Lessons_Refresh", GetLessonsDataAsync, LoadFromCacheAsync);
        IsLoading = false;
    }

    private async Task GetLessonsDataAsync() {
        var lessonsPath = Path.Combine("Data", "Lessons.json");
        await FileStorage.RemoveFileAsync(lessonsPath);
        await FileStorage.RemoveDirectoryAsync(Path.Combine("Data", "Marks"));

        Lessons = null;

        var lessons = await _webAccount.GetLessonsAsync();
        if (lessons is not null) {
            Lessons = lessons;
            await FileStorage.SaveAsync(Lessons, lessonsPath);
            await UpdateLastModifiedDate(lessonsPath);
        }
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "Lessons_Init", LoadLessonsAsync, LoadFromCacheAsync, "Lessons_Refresh");
        IsLoading = false;
    }

    private async Task LoadLessonsAsync() {
        var path = Path.Combine("Data", "Lessons.json");

        if (!await FileStorage.CheckExistsAsync(path))
            await GetLessonsDataAsync();
        else
            await LoadFromCacheAsync();
    }

    private async Task LoadFromCacheAsync() {
        var path = Path.Combine("Data", "Lessons.json");

        if (await FileStorage.CheckExistsAsync(path)) {
            var allLessonsButton = await FileStorage.GetAsync<List<string>>(path);
            allLessonsButton?.Add("Все");
            Lessons = allLessonsButton;
            await UpdateLastModifiedDate(path);
        }
    }

    private async Task UpdateLastModifiedDate(string filePath) {
        var lastModified = await FileStorage.GetLastModifiedAsync(filePath);
        if (lastModified.HasValue)
            LastUpdated = $"ОБНОВЛЕНО {lastModified.Value:dd.MM.yyyy HH:mm}";
        else
            LastUpdated = null;
    }
}
