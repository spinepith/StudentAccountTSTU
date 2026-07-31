using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

using WebAccount.Models;


namespace StudentAccountTSTU.ViewModels;

public partial class StudentMarkWrapper : ObservableObject {
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private List<string>? _marks;

    [ObservableProperty]
    private bool _isCurrentUser;
}

public partial class LessonMarkWrapper : ObservableObject {
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private List<string>? _headers;

    [ObservableProperty]
    private List<StudentMarkWrapper>? _students;
}

internal partial class MarksViewModel : ViewModelBase {
    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks;

    [ObservableProperty]
    private List<string>? _lessons;

    [ObservableProperty]
    private List<LessonMarkWrapper>? _marks;

    [ObservableProperty]
    private string? _currentLesson;

    [ObservableProperty]
    private bool _selectedLesson;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand))]
    [NotifyCanExecuteChangedFor(nameof(BackToLessonsCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _currentUserName;

    public MarksViewModel(Dictionary<string, Task?> activeLoadingTasks, WebAccount.WebAccount webAccount) {
        _activeLoadingTasks = activeLoadingTasks;
        _webAccount = webAccount;

        _ = InitializeDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;

        if (SelectedLesson && CurrentLesson is not null)
            await InitializeWithCacheAsync(_activeLoadingTasks, $"Marks_{CurrentLesson}", GetMarksDataAsync(CurrentLesson), () => LoadMarksFromCacheAsync(CurrentLesson));
        else
            await InitializeWithCacheAsync(_activeLoadingTasks, "Marks_Lessons", GetLessonsDataAsync(), LoadFromCacheAsync);

        IsLoading = false;
    }

    [RelayCommand]
    private async Task SelectLesson(string lessonName) {
        CurrentLesson = lessonName;
        SelectedLesson = true;

        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, $"Marks_{lessonName}", LoadMarksAsync(lessonName), () => LoadMarksFromCacheAsync(lessonName));
        IsLoading = false;
    }

    private async Task LoadMarksAsync(string lessonName) {
        if (lessonName is "Все") {
            var marksDirectory = Path.Combine("Data", "Marks");

            if (Lessons is not null && Lessons.All(lesson => lesson is "Все" || FileStorage.CheckExists(Path.Combine(marksDirectory, $"{lesson}.json"))))
                await LoadMarksFromCacheAsync(lessonName);
            else
                await GetMarksDataAsync(lessonName);
        }
        else {
            var path = Path.Combine("Data", "Marks", $"{lessonName}.json");
            if (!FileStorage.CheckExists(path))
                await GetMarksDataAsync(lessonName);
            else
                await LoadMarksFromCacheAsync(lessonName);
        }
    }

    private async Task LoadMarksFromCacheAsync(string lessonName) {
        if (lessonName is "Все") {
            var marksDirectory = Path.Combine("Data", "Marks");

            if (Lessons is not null && Lessons.All(lesson => lesson is "Все" || FileStorage.CheckExists(Path.Combine(marksDirectory, $"{lesson}.json")))) {
                var marksData = new List<Marks.Lesson>();
                foreach (var lesson in Lessons) {
                    if (lesson is "Все")
                        continue;

                    var lessonData = await FileStorage.GetAsync<Marks.Lesson>(Path.Combine(marksDirectory, $"{lesson}.json"));
                    if (lessonData is not null)
                        marksData.Add(lessonData);
                }

                Marks = ConvertToWrappedMarks(marksData);
            }
        }
        else {
            var path = Path.Combine("Data", "Marks", $"{lessonName}.json");
            if (FileStorage.CheckExists(path)) {
                var lessonData = await FileStorage.GetAsync<Marks.Lesson>(path);
                if (lessonData is not null)
                    Marks = ConvertToWrappedMarks(new List<Marks.Lesson> { lessonData });
            }
        }
    }

    private List<LessonMarkWrapper> ConvertToWrappedMarks(List<Marks.Lesson> lessons) {
        var result = new List<LessonMarkWrapper>();

        foreach (var lesson in lessons) {
            var wrappedLesson = new LessonMarkWrapper {
                Name     = lesson.Name,
                Headers  = lesson.Headers,
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

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private void BackToLessons() {
        SelectedLesson = false;
        CurrentLesson = null;
        Marks = null;
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        SelectedLesson = false;
        CurrentLesson = null;
        Marks = null;

        IsLoading = true;

        CurrentUserName = await _webAccount.GetUserNameAsync();
        await InitializeWithCacheAsync(_activeLoadingTasks, "Marks_Init", InitializeLessonsAsync(), LoadFromCacheAsync, "Marks_Lessons");

        IsLoading = false;
    }

    private async Task InitializeLessonsAsync() {
        var path = Path.Combine("Data", "Lessons.json");

        if (!FileStorage.CheckExists(path))
            await GetLessonsDataAsync();
        else
            await LoadFromCacheAsync();
    }

    private async Task LoadFromCacheAsync() {
        var path = Path.Combine("Data", "Lessons.json");

        if (FileStorage.CheckExists(path)) {
            var allLessonsButton = await FileStorage.GetAsync<List<string>>(path);
            allLessonsButton?.Add("Все");
            Lessons = allLessonsButton;
        }
    }

    private async Task GetLessonsDataAsync() {
        var lessonsPath = Path.Combine("Data", "Lessons.json");
        FileStorage.RemoveFile(lessonsPath);
        FileStorage.RemoveDirectory(Path.Combine("Data", "Marks"));

        Lessons = null;

        var lessons = await _webAccount.GetLessonsAsync();
        if (lessons is not null) {
            Lessons = lessons;
            await FileStorage.SaveAsync(Lessons, lessonsPath);
        }
    }

    private async Task GetMarksDataAsync(string lesson) {
        var marksDirectory = Path.Combine("Data", "Marks");

        Marks = null;

        List<Marks.Lesson>? lessonsData;
        if (lesson is "Все") {
            FileStorage.RemoveDirectory(marksDirectory);

            lessonsData = (await _webAccount.GetMarksAsync())?.LessonsMarks;
            if (lessonsData is not null)
                foreach (var lessonData in lessonsData)
                    await FileStorage.SaveAsync(lessonData, Path.Combine(marksDirectory, $"{lessonData.Name}.json"));
        }
        else {
            FileStorage.RemoveFile(Path.Combine(marksDirectory, $"{lesson}.json"));
            var marksData = await _webAccount.GetMarksAsync(lesson);
            if (marksData?.LessonsMarks is not null && marksData.LessonsMarks.Count > 0) {
                await FileStorage.SaveAsync(marksData.LessonsMarks[0], Path.Combine(marksDirectory, $"{lesson}.json"));
                lessonsData = marksData.LessonsMarks;
            }
            else {
                lessonsData = null;
            }
        }

        if (lessonsData is not null)
            Marks = ConvertToWrappedMarks(lessonsData);
    }
}
