using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

using WebAccount.Models;


namespace StudentAccountTSTU.ViewModels;

internal class SemesterGroup {
    public string? SemesterName { get; init; }
    public List<ReportCard.Exam> Exams { get; init; } = new();
}

internal partial class ReportCardViewModel : ViewModelBase {
    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks;

    [ObservableProperty]
    private ReportCard? _reportCard;

    [ObservableProperty]
    private IEnumerable<SemesterGroup>? _grouppedExams;

    [ObservableProperty]
    private IEnumerable<SemesterGroup>? _grouppedTests;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _lastUpdated;

    internal ReportCardViewModel(Dictionary<string, Task?> activeLoadingTasks, WebAccount.WebAccount webAccount) {
        _activeLoadingTasks = activeLoadingTasks;
        _webAccount = webAccount;

        _ = InitializeDataAsync();
    }

    partial void OnReportCardChanged(ReportCard? value) {
        if (value is not null) {
            GrouppedExams = GroupExamsBySemester(value.Exams);
            GrouppedTests = GroupExamsBySemester(value.Tests);
        }
        else {
            GrouppedExams = null;
            GrouppedTests = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "ReportCard_Refresh", GetReportCardDataAsync, LoadFromCacheAsync);
        IsLoading = false;
    }

    private async Task GetReportCardDataAsync() {
        FileStorage.RemoveFile(Path.Combine("Data", "ReportCard.json"));
        ReportCard = null;

        var reportCard = await _webAccount.GetReportCardAsync();
        if (reportCard is not null) {
            ReportCard = reportCard;
            await FileStorage.SaveAsync(ReportCard, Path.Combine("Data", "ReportCard.json"));
            UpdateLastModifiedDate(Path.Combine("Data", "ReportCard.json"));
        }
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        IsLoading = true;

        await InitializeWithCacheAsync(_activeLoadingTasks, "ReportCard_Init", LoadReportCardAsync, LoadFromCacheAsync, "ReportCard_Refresh");

        IsLoading = false;
    }

    private async Task LoadReportCardAsync() {
        var path = Path.Combine("Data", "ReportCard.json");

        if (!FileStorage.CheckExists(path))
            await GetReportCardDataAsync();
        else
            await LoadFromCacheAsync();
    }

    private async Task LoadFromCacheAsync() {
        var path = Path.Combine("Data", "ReportCard.json");

        if (FileStorage.CheckExists(path)) {
            ReportCard = await FileStorage.GetAsync<ReportCard>(path);
            UpdateLastModifiedDate(path);
        }
    }

    private void UpdateLastModifiedDate(string filePath) {
        var lastModified = Services.FileStorage.GetLastModified(filePath);
        if (lastModified.HasValue)
            LastUpdated = $"намнбкемн {lastModified.Value:dd.MM.yyyy HH:mm}";
        else
            LastUpdated = null;
    }

    private IEnumerable<SemesterGroup>? GroupExamsBySemester(IReadOnlyList<ReportCard.Exam>? semester) {
        if (semester is null)
            return null;

        var groupedList = new List<SemesterGroup>();
        
        foreach (var exam in semester) {
            SemesterGroup? targetGroup = null;

            foreach (var group in groupedList)
                if (group.SemesterName == exam.Semester) {
                    targetGroup = group;
                    break;
                }

            if (targetGroup is null) {
                targetGroup = new SemesterGroup { SemesterName = exam.Semester };
                groupedList.Add(targetGroup);
            }

            targetGroup.Exams.Add(exam);
        }

        return groupedList;
    }
}
