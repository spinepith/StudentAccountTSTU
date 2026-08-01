using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

using WebAccount.Models;


namespace StudentAccountTSTU.ViewModels;

internal partial class StudentMarkWrapper : ObservableObject {
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private List<string>? _marks;

    [ObservableProperty]
    private bool _isCurrentUser;
}

internal partial class LessonMarkWrapper : ObservableObject {
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private List<string>? _headers;

    [ObservableProperty]
    private List<StudentMarkWrapper>? _students;
}

internal partial class MarksViewModel : ViewModelBase {
    private readonly LessonsViewModel _parentViewModel;

    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks;
    private readonly string _lessonName;

    [ObservableProperty]
    private List<LessonMarkWrapper>? _marks;

    [ObservableProperty]
    private string? _currentLesson;

    [ObservableProperty]
    private string? _currentUserName;

    [ObservableProperty]
    private string? _lastUpdated;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand), nameof(BackToLessonsCommand))]
    private bool _isLoading;

    internal MarksViewModel(Dictionary<string, Task?> activeLoadingTasks, WebAccount.WebAccount webAccount, string lessonName, LessonsViewModel parentViewModel) {
        _activeLoadingTasks = activeLoadingTasks;
        _webAccount = webAccount;
        _lessonName = lessonName;
        _parentViewModel = parentViewModel;

        CurrentLesson = _lessonName;

        _ = InitializeDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, $"Marks_{CurrentLesson}_Refresh", GetMarksDataAsync(), LoadFromCacheAsync);
        IsLoading = false;
    }

    private async Task GetMarksDataAsync() {
        var marksDirectory = Path.Combine("Data", "Marks");
        Marks = null;

        List<Marks.Lesson>? lessonsData;
        if (_lessonName is "Все") {
            FileStorage.RemoveDirectory(marksDirectory);

            lessonsData = (await _webAccount.GetMarksAsync())?.LessonsMarks;
            if (lessonsData is not null)
                foreach (var lessonData in lessonsData)
                    await FileStorage.SaveAsync(lessonData, Path.Combine(marksDirectory, $"{lessonData.Name}.json"));
        }
        else {
            FileStorage.RemoveFile(Path.Combine(marksDirectory, $"{_lessonName}.json"));
            var marksData = await _webAccount.GetMarksAsync(_lessonName);
            if (marksData?.LessonsMarks is not null && marksData.LessonsMarks.Count > 0) {
                await FileStorage.SaveAsync(marksData.LessonsMarks[0], Path.Combine(marksDirectory, $"{_lessonName}.json"));
                lessonsData = marksData.LessonsMarks;
            }
            else
                lessonsData = null;
        }

        if (lessonsData is not null) {
            Marks = ConvertToWrappedMarks(lessonsData);
            if (_lessonName is not "Все") {
                var filePath = Path.Combine(marksDirectory, $"{_lessonName}.json");
                UpdateLastModifiedDate(filePath);
            }
        }
    }

    private List<LessonMarkWrapper> ConvertToWrappedMarks(List<Marks.Lesson> lessons) {
        var result = new List<LessonMarkWrapper>();

        foreach (var lesson in lessons) {
            var wrappedLesson = new LessonMarkWrapper {
                Name = lesson.Name,
                Headers = lesson.Headers,
                Students = lesson.Students?.Select(
                    student => new StudentMarkWrapper {
                        Name = student.Name,
                        Marks = student.Marks,
                        IsCurrentUser = string.Equals(student.Name, CurrentUserName, System.StringComparison.OrdinalIgnoreCase)
                    }
                ).ToList()
            };
            result.Add(wrappedLesson);
        }

        return result;
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        IsLoading = true;
        CurrentUserName = await _webAccount.GetUserNameAsync();
        await InitializeWithCacheAsync(_activeLoadingTasks, $"Marks_{CurrentLesson}_Init", LoadMarksAsync(), LoadFromCacheAsync, $"Marks_{CurrentLesson}_Refresh");
        IsLoading = false;
    }

    private async Task LoadMarksAsync() {
        if (_lessonName is "Все") {
            var marksDirectory = Path.Combine("Data", "Marks");
            var lessonsPath = Path.Combine("Data", "Lessons.json");

            if (FileStorage.CheckExists(lessonsPath)) {
                var lessons = await FileStorage.GetAsync<List<string>>(lessonsPath);
                if (lessons is not null && lessons.All(lesson => FileStorage.CheckExists(Path.Combine(marksDirectory, $"{lesson}.json"))))
                    await LoadFromCacheAsync();
                else
                    await GetMarksDataAsync();
            }
            else
                await GetMarksDataAsync();
        }
        else {
            var path = Path.Combine("Data", "Marks", $"{_lessonName}.json");
            if (!FileStorage.CheckExists(path))
                await GetMarksDataAsync();
            else
                await LoadFromCacheAsync();
        }
    }

    private async Task LoadFromCacheAsync() {
        if (_lessonName is "Все") {
            var marksDirectory = Path.Combine("Data", "Marks");
            var lessonsPath = Path.Combine("Data", "Lessons.json");

            if (FileStorage.CheckExists(lessonsPath)) {
                var lessons = await FileStorage.GetAsync<List<string>>(lessonsPath);
                if (lessons is not null) {
                    var marksData = new List<Marks.Lesson>();
                    foreach (var lesson in lessons) {
                        var lessonData = await FileStorage.GetAsync<Marks.Lesson>(Path.Combine(marksDirectory, $"{lesson}.json"));
                        if (lessonData is not null)
                            marksData.Add(lessonData);
                    }
                    Marks = ConvertToWrappedMarks(marksData);
                }
            }
        }
        else {
            var path = Path.Combine("Data", "Marks", $"{_lessonName}.json");
            if (FileStorage.CheckExists(path)) {
                var lessonData = await FileStorage.GetAsync<Marks.Lesson>(path);
                if (lessonData is not null) {
                    Marks = ConvertToWrappedMarks(new List<Marks.Lesson> { lessonData });
                    UpdateLastModifiedDate(path);
                }
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
    private void BackToLessons() {
        _parentViewModel.BackToLessons();
    }
}
