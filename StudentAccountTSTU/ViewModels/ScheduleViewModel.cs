using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

using WebAccount.Models;


namespace StudentAccountTSTU.ViewModels;

internal class DayGroup {
    public string? DayName               { get; init; }
    public bool IsHighlited              { get; init; }
    public List<Schedule.Lesson> Lessons { get; init; } = new();
}

internal partial class ScheduleViewModel : ViewModelBase {
    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks;

    [ObservableProperty]
    private Schedule? _schedule;

    [ObservableProperty]
    private bool _customSchedule;

    [ObservableProperty]
    private IEnumerable<DayGroup>? _groupedOddWeek;

    [ObservableProperty]
    private IEnumerable<DayGroup>? _groupedEvenWeek;

    [ObservableProperty]
    private IEnumerable<DayGroup>? _currentWeekSchedule;

    [ObservableProperty]
    private string? _currentWeek;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _lastUpdated;

    [ObservableProperty]
    private Bitmap? _firstTypeImage;

    [ObservableProperty]
    private Bitmap? _secondTypeFirstImage;

    [ObservableProperty]
    private Bitmap? _secondTypeSecondImage;

    [ObservableProperty]
    private bool _showCustomSchedule;

    [ObservableProperty]
    private bool _showWeeksHeader;

    internal ScheduleViewModel(Dictionary<string, Task?> activeLoadingTasks, WebAccount.WebAccount webAccount, Settings settings) {
        _activeLoadingTasks = activeLoadingTasks;
        _webAccount = webAccount;

        CustomSchedule = settings.CustomSchedule;

        _ = InitializeDataAsync();
    }

    partial void OnScheduleChanged(Schedule? value) {
        if (value is not null) {
            var week = value.CurrentWeek!.Split()[^1].Trim();
            var isOddWeek = week is "НЕЧЕТНАЯ";

            CurrentWeek = isOddWeek ? "НЕЧЕТНАЯ" : "ЧЕТНАЯ";
            GroupedOddWeek = GroupLessonsByDay(value.OddWeek, isOddWeek);
            GroupedEvenWeek = GroupLessonsByDay(value.EvenWeek, !isOddWeek);
            CurrentWeekSchedule = isOddWeek ? GroupedOddWeek : GroupedEvenWeek;
        }
        else {
            CurrentWeek = null;
            GroupedOddWeek = null;
            GroupedEvenWeek = null;
            CurrentWeekSchedule = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "Schedule_Refresh", GetScheduleDataAsync, LoadFromCacheAsync);
        IsLoading = false;
    }

    private async Task GetScheduleDataAsync() {
        FileStorage.RemoveFile(Path.Combine("Data", "Schedule.json"));
        Schedule = null;

        var schedule = await _webAccount.GetScheduleAsync();
        if (schedule is not null) {
            Schedule = schedule;
            await FileStorage.SaveAsync(Schedule, Path.Combine("Data", "Schedule.json"));
            UpdateLastModifiedDate(Path.Combine("Data", "Schedule.json"));
        }
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "Schedule_Init", LoadScheduleAsync, LoadFromCacheAsync, "Schedule_Refresh");
        IsLoading = false;
    }

    private async Task LoadScheduleAsync() {
        var firstTypePath = Path.Combine("Data", "ScheduleFirstType.jpg");
        var secondType1Path = Path.Combine("Data", "ScheduleSecondType1.jpg");
        var secondType2Path = Path.Combine("Data", "ScheduleSecondType2.jpg");

        var firstTypeExists = FileStorage.CheckExists(firstTypePath);
        var secondType1Exists = FileStorage.CheckExists(secondType1Path);
        var secondType2Exists = FileStorage.CheckExists(secondType2Path);

        var hasSecondType = secondType1Exists || secondType2Exists;
        var hasFirstType = firstTypeExists;

        if (hasSecondType) {
            ShowCustomSchedule = true;
            ShowWeeksHeader = true;
            if (secondType1Exists)
                SecondTypeFirstImage = new Bitmap(FileStorage.GetFullPath(secondType1Path));
            if (secondType2Exists)
                SecondTypeSecondImage = new Bitmap(FileStorage.GetFullPath(secondType2Path));
        }
        else if (hasFirstType) {
            ShowCustomSchedule = true;
            ShowWeeksHeader = false;
            FirstTypeImage = new Bitmap(FileStorage.GetFullPath(firstTypePath));
        }
        else {
            ShowCustomSchedule = false;
            ShowWeeksHeader = true;
        }

        var path = Path.Combine("Data", "Schedule.json");
        bool hasCache = FileStorage.CheckExists(path);

        if (hasCache) {
            await LoadFromCacheAsync();
        }

        // Only run parser if cache is missing and we don't have a custom schedule that overrides it
        bool needParser = !hasCache && !ShowCustomSchedule;

        if (needParser) {
            await GetScheduleDataAsync();
        }
    }

    private async Task LoadFromCacheAsync() {
        var path = Path.Combine("Data", "Schedule.json");

        if (FileStorage.CheckExists(path)) {
            Schedule = await FileStorage.GetAsync<Schedule>(path);
            UpdateLastModifiedDate(path);
        }
    }

    private void UpdateLastModifiedDate(string filePath) {
        var lastModified = Services.FileStorage.GetLastModified(filePath);
        if (lastModified.HasValue && !ShowCustomSchedule)
            LastUpdated = $"ОБНОВЛЕНО {lastModified.Value:dd.MM.yyyy HH:mm}";
        else
            LastUpdated = null;
    }

    private IEnumerable<DayGroup>? GroupLessonsByDay(IReadOnlyList<Schedule.Lesson>? lessons, bool isCurrentWeek) {
        if (lessons is null)
            return null;

        var groupedList = new List<DayGroup>();
        string today = DateTime.Now.DayOfWeek switch {
            DayOfWeek.Monday    => "пн",
            DayOfWeek.Tuesday   => "вт",
            DayOfWeek.Wednesday => "ср",
            DayOfWeek.Thursday  => "чт",
            DayOfWeek.Friday    => "пт",
            DayOfWeek.Saturday  => "сб",
            DayOfWeek.Sunday    => "вс",
            _                   => ""
        };

        foreach (var lesson in lessons) {
            if (lesson.Day is null || string.IsNullOrEmpty(lesson.Day))
                continue;

            DayGroup? targetGroup = null;

            foreach (var group in groupedList)
                if (group.DayName == lesson.Day) {
                    targetGroup = group;
                    break;
                }

            if (targetGroup is null) {
                targetGroup = new DayGroup {
                    DayName = lesson.Day,
                    IsHighlited = isCurrentWeek && string.Equals(lesson.Day, today, StringComparison.OrdinalIgnoreCase)
                };
                groupedList.Add(targetGroup);
            }

            targetGroup.Lessons.Add(lesson);
        }

        return groupedList;
    }
}
