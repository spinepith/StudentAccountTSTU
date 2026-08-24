using System;
using System.IO;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;
using StudentAccountTSTU.Services.Storage;

namespace StudentAccountTSTU.ViewModels.SettingsViewModels;

internal partial class CustomAvatarViewModel : ViewModelBase {
    private readonly SettingsViewModel _parentViewModel;

    [ObservableProperty]
    private Settings _settings;

    [ObservableProperty]
    private byte[]? _tempImageBytes;

    [ObservableProperty]
    private Bitmap? _selectedImageBitmap;

    [ObservableProperty]
    private bool _avatarExists;

    internal CustomAvatarViewModel(SettingsViewModel parentViewModel, Settings settings) {
        _parentViewModel = parentViewModel;
        Settings = settings;

        _ = CheckAvatarExists();
    }

    private async Task CheckAvatarExists() {
        AvatarExists = await FileStorage.CheckExistsAsync(Path.Combine("Data", "Avatar.jpg"));

        if (AvatarExists) {
            try {
                using var stream = await FileStorage.GetFileStreamAsync(Path.Combine("Data", "Avatar.jpg"));
                SelectedImageBitmap = new Bitmap(stream);
            }
            catch {
                SelectedImageBitmap = null;
            }
        }

        Settings.CustomAvatar = AvatarExists;
    }

    private void LoadBitmap(byte[] data) {
        try {
            using var ms = new MemoryStream(data);
            SelectedImageBitmap = new Bitmap(ms);
        }
        catch {
            SelectedImageBitmap = null;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SelectImage() {
        if (PlatformHooks.NativeGaleryAction is not null) {
            var photoPath = await PlatformHooks.NativeGaleryAction.Invoke();

            if (photoPath is not null) {
                using var stream = File.OpenRead(photoPath);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                TempImageBytes = ms.ToArray();
                LoadBitmap(TempImageBytes);
            }
            return;
        }
        else {
            Avalonia.Controls.TopLevel? topLevel = App.TopLevel;
            if (topLevel == null && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                topLevel = desktop.MainWindow;

            if (topLevel?.StorageProvider is not { } storageProvider)
                return;

            var options = new FilePickerOpenOptions {
                Title = "Выберите фото",
                AllowMultiple = false,
                FileTypeFilter = new[] {
                    new FilePickerFileType("Изображения") {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" },
                        MimeTypes = new[] { "image/*" }
                    }
                }
            };

            var result = await storageProvider.OpenFilePickerAsync(options);

            if (result.Count > 0) {
                var file = result[0];
                using var stream = await file.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                TempImageBytes = ms.ToArray();
                LoadBitmap(TempImageBytes);
            }
        }
    }

    [RelayCommand]
    private async Task Apply() {
        if (TempImageBytes is not null) {
            using var stream = new MemoryStream(TempImageBytes);
            await FileStorage.SaveStreamAsync(stream, Path.Combine("Data", "Avatar.jpg"));
            TempImageBytes = null;
            await CheckAvatarExists();
        }
    }

    [RelayCommand]
    private async Task Delete() {
        await FileStorage.RemoveFileAsync(Path.Combine("Data", "Avatar.jpg"));
        SelectedImageBitmap = null;
        TempImageBytes = null;
        await CheckAvatarExists();
    }

    [RelayCommand]
    private void BackToAllSettings() {
        _parentViewModel.BackToAllSettings();
    }
}
