using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

using WebAccount.Interfaces;
using WebAccount.Models;


namespace StudentAccountTSTU.ViewModels;

internal partial class UserDataViewModel : ViewModelBase {
    private readonly MainViewModel _mainViewModel;
    private readonly IHttpService _httpService;
    private readonly WebAccount.WebAccount _webAccount;


    [ObservableProperty]
    private UserData? _userData;

    [ObservableProperty]
    private Bitmap? _userImage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetDataCommand))]
    private bool _isLoading;

    public UserDataViewModel(MainViewModel mainViewModel, IHttpService httpService, WebAccount.WebAccount webAccount) {
        _mainViewModel = mainViewModel;
        _httpService = httpService;
        _webAccount = webAccount;

        _ = InitializeDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task GetData() {
        IsLoading = true;

        FileStorage.Remove(Path.Combine("Data", "UserData.json"));
        FileStorage.Remove(Path.Combine("Data", "UserImage.jpg"));
        UserData = null;
        UserImage = null;

        var userData = await _webAccount.GetUserDataAsync();
        if (userData is not null) {
            UserData = userData;
            await FileStorage.SaveAsync(UserData, Path.Combine("Data", "UserData.json"));

            if (!string.IsNullOrEmpty(UserData.Image))
                UserImage = await LoadImageAsync(Path.Combine("Data", "UserImage.jpg"), UserData.Image, true);
        }

        IsLoading = false;
    }

    private bool CanUpdate() => !IsLoading;

    private async Task InitializeDataAsync() {
        var path = Path.Combine("Data", "UserData.json");
        var imagePath = Path.Combine("Data", "UserImage.jpg");

        if (!FileStorage.CheckExists(path))
            await GetData();
        else {
            UserData = await FileStorage.GetAsync<UserData>(path);
            if (UserData is not null && !string.IsNullOrEmpty(UserData.Image))
                UserImage = await LoadImageAsync(imagePath, UserData.Image, false);
        }
    }

    private async Task<Bitmap?> LoadImageAsync(string path, string url, bool download) {
        try {
            if (download) {
                using var networkStream = await _httpService.GetStreamAsync(url);
                await FileStorage.SaveStreamAsync(networkStream, path);
            }

            var loadPath = FileStorage.GetFullPath(path);
            return new Bitmap(loadPath);
        }
        catch {
            if (FileStorage.CheckExists(path))
                FileStorage.Remove(path);
        }

        return null;
    }
}
