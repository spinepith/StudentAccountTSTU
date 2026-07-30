using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Avalonia.Data.Converters;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

using WebAccount.Models;


namespace StudentAccountTSTU.ViewModels;


public class SemesterGroup {
    public string? SemesterName { get; init; }
    public List<ReportCard.Exam> Exams { get; init; } = new();
}

internal partial class ReportCardViewModel : ViewModelBase {
    private readonly WebAccount.WebAccount _webAccount;

    [ObservableProperty]
    private ReportCard? _reportCard;

    [ObservableProperty]
    private IEnumerable<SemesterGroup>? _grouppedExams;

    [ObservableProperty]
    private IEnumerable<SemesterGroup>? _grouppedTests;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand))]
    private bool _isLoading;

    public ReportCardViewModel(WebAccount.WebAccount webAccount) {
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

        FileStorage.RemoveFile(Path.Combine("Data", "ReportCard.json"));
        ReportCard = null;

        var reportCard = await _webAccount.GetReportCardAsync();
        if (reportCard is not null) {
            ReportCard = reportCard;
            await FileStorage.SaveAsync(ReportCard, Path.Combine("Data", "ReportCard.json"));
        }

        IsLoading = false;
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        var path = Path.Combine("Data", "ReportCard.json");

        if (!FileStorage.CheckExists(path))
            await GetData();
        else
            ReportCard = await FileStorage.GetAsync<ReportCard>(path);
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
