using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services.Storage;

namespace StudentAccountTSTU.ViewModels;

internal partial class GroupsViewModel : ViewModelBase {
    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private List<string>? groups;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _lastUpdated;

    internal GroupsViewModel(Dictionary<string, Task?> activeLoadingTasks, WebAccount.WebAccount webAccount) {
        _activeLoadingTasks = activeLoadingTasks;
        _webAccount = webAccount;

        _ = InitializeDataAsync();
    }

    [RelayCommand]
    private void SelectGroup(string groupName) {
        CurrentPage = new RatingViewModel(_activeLoadingTasks, _webAccount, groupName, this);
    }

    internal void BackToGroups() {
        CurrentPage = null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "Groups_Refresh", GetGroupsDataAsync, LoadFromCacheAsync);
        IsLoading = false;
    }

    private async Task GetGroupsDataAsync() {
        var groupsPath = Path.Combine("Data", "Groups.json");
        await FileStorage.RemoveFileAsync(groupsPath);
        await FileStorage.RemoveDirectoryAsync(Path.Combine("Data", "Ratings"));

        Groups = null;

        var groups = await _webAccount.GetGroupsAsync();
        if (groups is not null) {
            Groups = groups;
            await FileStorage.SaveAsync(Groups, groupsPath);
            await UpdateLastModifiedDate(groupsPath);
        }
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "Groups_Init", LoadGroupsAsync, LoadFromCacheAsync, "Groups_Refresh");
        IsLoading = false;
    }

    private async Task LoadGroupsAsync() {
        var path = Path.Combine("Data", "Groups.json");

        if (!await FileStorage.CheckExistsAsync(path))
            await GetGroupsDataAsync();
        else
            await LoadFromCacheAsync();
    }

    private async Task LoadFromCacheAsync() {
        var path = Path.Combine("Data", "Groups.json");

        if (await FileStorage.CheckExistsAsync(path)) {
            Groups = await FileStorage.GetAsync<List<string>>(path);
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
