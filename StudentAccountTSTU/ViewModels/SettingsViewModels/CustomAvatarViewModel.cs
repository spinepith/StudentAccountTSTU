using System.IO;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

namespace StudentAccountTSTU.ViewModels.SettingsViewModels;

internal partial class CustomAvatarViewModel : ViewModelBase {
    private readonly SettingsViewModel _parentViewModel;

    [ObservableProperty]
    private Settings _settings;

    [ObservableProperty]
    private string? _tempImagePath;

    [ObservableProperty]
    private Bitmap? _selectedImageBitmap;

    [ObservableProperty]
    private bool _avatarExists;

    internal CustomAvatarViewModel(SettingsViewModel parentViewModel, Settings settings) {
        _parentViewModel = parentViewModel;
        Settings = settings;

        CheckAvatarExists();
    }

    private void CheckAvatarExists() {
        AvatarExists = FileStorage.CheckExists(Path.Combine("Data", "Avatar.jpg"));

        if (AvatarExists) {
            var path = FileStorage.GetFullPath(Path.Combine("Data", "Avatar.jpg"));
            LoadBitmap(path);
        }

        Settings.CustomAvatar = AvatarExists;
    }

    private void LoadBitmap(string path) {
        try {
            SelectedImageBitmap = new Bitmap(path);
        }
        catch {
            SelectedImageBitmap = null;
        }
    }

    [RelayCommand]
    private async Task SelectImage() {
        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var topLevel = lifetime?.MainWindow;

        if (topLevel?.StorageProvider is not { } storageProvider)
            return;

        var options = new FilePickerOpenOptions {
            Title = "Выбрать аватар",
            AllowMultiple = false,
            FileTypeFilter = new[] {
                new FilePickerFileType("Изображения") {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }
                }
            }
        };

        var result = await storageProvider.OpenFilePickerAsync(options);

        if (result.Count > 0) {
            TempImagePath = result[0].Path.LocalPath;
            LoadBitmap(TempImagePath);
        }
    }

    [RelayCommand]
    private async Task Apply() {
        if (TempImagePath is not null) {
            using var stream = File.OpenRead(TempImagePath);
            await FileStorage.SaveStreamAsync(stream, Path.Combine("Data", "Avatar.jpg"));
            TempImagePath = null;
            CheckAvatarExists();
        }
    }

    [RelayCommand]
    private void Delete() {
        FileStorage.RemoveFile(Path.Combine("Data", "Avatar.jpg"));
        SelectedImageBitmap = null;
        TempImagePath = null;
        CheckAvatarExists();
    }

    [RelayCommand]
    private void BackToAllSettings() {
        _parentViewModel.BackToAllSettings();
    }
}
