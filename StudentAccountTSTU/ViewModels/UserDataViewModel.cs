using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services.Storage;

using WebAccount.Interfaces;
using WebAccount.Models;


namespace StudentAccountTSTU.ViewModels;

internal partial class UserDataViewModel : ViewModelBase {
    private readonly IHttpService _httpService;
    private readonly WebAccount.WebAccount _webAccount;
    private readonly Dictionary<string, Task?> _activeLoadingTasks;


    [ObservableProperty]
    private UserData? _userData;

    [ObservableProperty]
    private string _greeting = "Здравствуйте!";

    partial void OnUserDataChanged(UserData? value) {
        if (value?.FioGroup is string fio) {
            var parts = fio.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                Greeting = parts[0].Trim();
        }
    }

    [ObservableProperty]
    private Bitmap? _userImage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string? _lastUpdated;

    internal UserDataViewModel(Dictionary<string, Task?> activeLoadingTasks, IHttpService httpService, WebAccount.WebAccount webAccount) {
        _activeLoadingTasks = activeLoadingTasks;
        _httpService = httpService;
        _webAccount = webAccount;

        _ = InitializeDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "UserData_Refresh", GetUserDataAsync, LoadFromCacheAsync);
        IsLoading = false;
    }

    private async Task GetUserDataAsync() {
        var userDataPath = Path.Combine("Data", "UserData.json");
        var fullPath = FileStorage.GetFullPath(userDataPath);

        await FileStorage.RemoveFileAsync(Path.Combine("Data", "UserData.json"));
        await FileStorage.RemoveFileAsync(Path.Combine("Data", "UserImage.jpg"));
        UserData = null;
        UserImage = null;

        var userData = await _webAccount.GetUserDataAsync();
        if (userData is not null) {
            UserData = userData;

            await FileStorage.SaveAsync(UserData, Path.Combine("Data", "UserData.json"));


            if (!string.IsNullOrEmpty(UserData.Image))
                UserImage = await LoadImageAsync(Path.Combine("Data", "UserImage.jpg"), UserData.Image, true);

            await UpdateLastModifiedDate(Path.Combine("Data", "UserData.json"));
        }
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        IsLoading = true;
        await InitializeWithCacheAsync(_activeLoadingTasks, "UserData_Init", LoadUserDataAsync, LoadFromCacheAsync, "UserData_Refresh");
        IsLoading = false;
    }

    private async Task LoadUserDataAsync() {
        var path = Path.Combine("Data", "UserData.json");
        var imagePath = Path.Combine("Data", "UserImage.jpg");

        if (!await FileStorage.CheckExistsAsync(path))
            await GetUserDataAsync();
        else
            await LoadFromCacheAsync();
    }

    private async Task LoadFromCacheAsync() {
        var path = Path.Combine("Data", "UserData.json");
        var imagePath = Path.Combine("Data", "UserImage.jpg");

        if (await FileStorage.CheckExistsAsync(path)) {
            UserData = await FileStorage.GetAsync<UserData>(path);
            if (UserData is not null && !string.IsNullOrEmpty(UserData.Image))
                UserImage = await LoadImageAsync(imagePath, UserData.Image, false);
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

    private async Task<Bitmap?> LoadImageAsync(string path, string url, bool download) {
        try {
            if (download) {
                using var networkStream = await _httpService.GetStreamAsync(url);
                await FileStorage.SaveStreamAsync(networkStream, path);
            }

            using var fileStream = await FileStorage.GetFileStreamAsync(path);
            return new Bitmap(fileStream);
        }
        catch {
            if (await FileStorage.CheckExistsAsync(path))
                await FileStorage.RemoveFileAsync(path);
        }

        return null;
    }
}
