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

internal partial class CustomScheduleViewModel : ViewModelBase {
    /* ЭТА СТРАНИЦА НАПИСАНА ПОЛНОСТЬЮ CLAUDE CODE */
    private readonly SettingsViewModel _parentViewModel;

    [ObservableProperty]
    private Settings _settings;

    [ObservableProperty]
    private bool _isFirstTypeSelected = true;

    [ObservableProperty]
    private bool _isSecondTypeSelected;

    [ObservableProperty]
    private byte[]? _tempFirstTypeImageBytes;

    [ObservableProperty]
    private byte[]? _tempSecondTypeFirstImageBytes;

    [ObservableProperty]
    private byte[]? _tempSecondTypeSecondImageBytes;

    [ObservableProperty]
    private Bitmap? _firstTypeImage;

    [ObservableProperty]
    private Bitmap? _secondTypeFirstImage;

    [ObservableProperty]
    private Bitmap? _secondTypeSecondImage;

    [ObservableProperty]
    private bool _showSecondTypeDefault;

    [ObservableProperty]
    private bool _canDelete;

    [ObservableProperty]
    private bool _canApply;

    internal CustomScheduleViewModel(SettingsViewModel parentViewModel, Settings settings) {
        _parentViewModel = parentViewModel;
        Settings = settings;

        _ = CheckImagesExist();

        if (OperatingSystem.IsBrowser()) {
            try {
                Avalonia.Controls.TopLevel? topLevel = App.TopLevel;
                if (topLevel == null && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    topLevel = desktop.MainWindow;

                if (topLevel?.StorageProvider is not null)
                    _ = topLevel.StorageProvider.OpenFileBookmarkAsync("dummy_warmup_for_ios");
            }
            catch { }
        }
    }

    partial void OnIsFirstTypeSelectedChanged(bool value) {
        if (value) {
            IsSecondTypeSelected = false;
            _ = CheckImagesExist();
        }
    }

    partial void OnIsSecondTypeSelectedChanged(bool value) {
        if (value) {
            IsFirstTypeSelected = false;
            _ = CheckImagesExist();
        }
    }

    private Bitmap? LoadBitmap(byte[] data) {
        try {
            using var ms = new MemoryStream(data);
            return new Bitmap(ms);
        }
        catch {
            return null;
        }
    }

    private async Task<Bitmap?> LoadBitmapFromStorage(string path) {
        try {
            using var stream = await FileStorage.GetFileStreamAsync(path);
            return new Bitmap(stream);
        }
        catch {
            return null;
        }
    }

    private async Task CheckImagesExist() {
        var firstTypeExists = await FileStorage.CheckExistsAsync(Path.Combine("Data", "ScheduleFirstType.jpg"));
        var secondType1Exists = await FileStorage.CheckExistsAsync(Path.Combine("Data", "ScheduleSecondType1.jpg"));
        var secondType2Exists = await FileStorage.CheckExistsAsync(Path.Combine("Data", "ScheduleSecondType2.jpg"));

        if (TempFirstTypeImageBytes is not null)
            FirstTypeImage = LoadBitmap(TempFirstTypeImageBytes);
        else if (firstTypeExists)
            FirstTypeImage = await LoadBitmapFromStorage(Path.Combine("Data", "ScheduleFirstType.jpg"));
        else
            FirstTypeImage = null;

        if (TempSecondTypeFirstImageBytes is not null)
            SecondTypeFirstImage = LoadBitmap(TempSecondTypeFirstImageBytes);
        else if (secondType1Exists)
            SecondTypeFirstImage = await LoadBitmapFromStorage(Path.Combine("Data", "ScheduleSecondType1.jpg"));
        else
            SecondTypeFirstImage = null;

        if (TempSecondTypeSecondImageBytes is not null)
            SecondTypeSecondImage = LoadBitmap(TempSecondTypeSecondImageBytes);
        else if (secondType2Exists)
            SecondTypeSecondImage = await LoadBitmapFromStorage(Path.Combine("Data", "ScheduleSecondType2.jpg"));
        else
            SecondTypeSecondImage = null;

        ShowSecondTypeDefault = SecondTypeFirstImage is null && SecondTypeSecondImage is null;
        CanDelete = (IsFirstTypeSelected && firstTypeExists) || (IsSecondTypeSelected && (secondType1Exists || secondType2Exists));
        UpdateCanApply();
    }

    private async Task<byte[]?> SelectImage(string title) {
        if (PlatformHooks.NativeGaleryAction is not null) {
            var photoPath = await PlatformHooks.NativeGaleryAction.Invoke();

            if (photoPath is not null) {
                using var stream = File.OpenRead(photoPath);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
            return null;
        }
        else {
            Avalonia.Controls.TopLevel? topLevel = App.TopLevel;
            if (topLevel == null && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                topLevel = desktop.MainWindow;

            if (topLevel?.StorageProvider is not { } storageProvider)
                return null;

            var options = new FilePickerOpenOptions {
                Title = title,
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
                return ms.ToArray();
            }
            return null;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SelectFirstTypeImage() {
        TempFirstTypeImageBytes = await SelectImage("Выберите расписание");
        if (TempFirstTypeImageBytes is not null) {
            FirstTypeImage = LoadBitmap(TempFirstTypeImageBytes);
            CanApply = true;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SelectSecondTypeFirstImage() {
        TempSecondTypeFirstImageBytes = await SelectImage("Выберите расписание числителя");
        if (TempSecondTypeFirstImageBytes is not null) {
            SecondTypeFirstImage = LoadBitmap(TempSecondTypeFirstImageBytes);
            ShowSecondTypeDefault = false;
            UpdateCanApply();
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SelectSecondTypeSecondImage() {
        TempSecondTypeSecondImageBytes = await SelectImage("Выберите расписание знаменателя");
        if (TempSecondTypeSecondImageBytes is not null) {
            SecondTypeSecondImage = LoadBitmap(TempSecondTypeSecondImageBytes);
            ShowSecondTypeDefault = false;
            UpdateCanApply();
        }
    }

    private void UpdateCanApply() {
        CanApply = (IsFirstTypeSelected && TempFirstTypeImageBytes is not null) ||
                   (IsSecondTypeSelected && (TempSecondTypeFirstImageBytes is not null || TempSecondTypeSecondImageBytes is not null));
    }

    [RelayCommand]
    private async Task Apply() {
        if (IsFirstTypeSelected && TempFirstTypeImageBytes is not null) {
            using var stream = new MemoryStream(TempFirstTypeImageBytes);
            await FileStorage.SaveStreamAsync(stream, Path.Combine("Data", "ScheduleFirstType.jpg"));
            TempFirstTypeImageBytes = null;
        }
        else if (IsSecondTypeSelected) {
            if (TempSecondTypeFirstImageBytes is not null) {
                using var stream = new MemoryStream(TempSecondTypeFirstImageBytes);
                await FileStorage.SaveStreamAsync(stream, Path.Combine("Data", "ScheduleSecondType1.jpg"));
                TempSecondTypeFirstImageBytes = null;
            }
            if (TempSecondTypeSecondImageBytes is not null) {
                using var stream = new MemoryStream(TempSecondTypeSecondImageBytes);
                await FileStorage.SaveStreamAsync(stream, Path.Combine("Data", "ScheduleSecondType2.jpg"));
                TempSecondTypeSecondImageBytes = null;
            }
        }

        Settings.CustomSchedule = true;
        CanApply = false;
        await CheckImagesExist();
    }

    [RelayCommand]
    private async Task Delete() {
        if (IsFirstTypeSelected) {
            await FileStorage.RemoveFileAsync(Path.Combine("Data", "ScheduleFirstType.jpg"));
            TempFirstTypeImageBytes = null;
            FirstTypeImage = null;
        }
        else if (IsSecondTypeSelected) {
            await FileStorage.RemoveFileAsync(Path.Combine("Data", "ScheduleSecondType1.jpg"));
            await FileStorage.RemoveFileAsync(Path.Combine("Data", "ScheduleSecondType2.jpg"));
            TempSecondTypeFirstImageBytes = null;
            TempSecondTypeSecondImageBytes = null;
            SecondTypeFirstImage = null;
            SecondTypeSecondImage = null;
            ShowSecondTypeDefault = true;
        }

        CanApply = false;
        await CheckImagesExist();
    }

    [RelayCommand]
    private void BackToAllSettings() {
        _parentViewModel.BackToAllSettings();
    }
}
