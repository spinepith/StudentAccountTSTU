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
    private Bitmap? _firstTypeImage;

    [ObservableProperty]
    private Bitmap? _secondTypeFirstImage;

    [ObservableProperty]
    private Bitmap? _secondTypeSecondImage;

    [ObservableProperty]
    private bool _showSecondTypeDefault = true;

    [ObservableProperty]
    private string? _tempFirstTypeImagePath;

    [ObservableProperty]
    private string? _tempSecondTypeFirstImagePath;

    [ObservableProperty]
    private string? _tempSecondTypeSecondImagePath;

    [ObservableProperty]
    private bool _canDelete;

    [ObservableProperty]
    private bool _canApply;

    internal CustomScheduleViewModel(SettingsViewModel parentViewModel, Settings settings) {
        _parentViewModel = parentViewModel;
        Settings = settings;

        CheckImagesExist();
    }

    partial void OnIsFirstTypeSelectedChanged(bool value) {
        if (value) {
            IsSecondTypeSelected = false;
            CheckImagesExist();
        }
    }

    partial void OnIsSecondTypeSelectedChanged(bool value) {
        if (value) {
            IsFirstTypeSelected = false;
            CheckImagesExist();
        }
    }

    private void CheckImagesExist() {
        var firstTypeExists = FileStorage.CheckExists(Path.Combine("Data", "ScheduleFirstType.jpg"));
        var secondType1Exists = FileStorage.CheckExists(Path.Combine("Data", "ScheduleSecondType1.jpg"));
        var secondType2Exists = FileStorage.CheckExists(Path.Combine("Data", "ScheduleSecondType2.jpg"));

        if (TempFirstTypeImagePath is not null)
            FirstTypeImage = new Bitmap(TempFirstTypeImagePath);
        else if (firstTypeExists)
            FirstTypeImage = new Bitmap(FileStorage.GetFullPath(Path.Combine("Data", "ScheduleFirstType.jpg")));
        else
            FirstTypeImage = null;

        if (TempSecondTypeFirstImagePath is not null)
            SecondTypeFirstImage = new Bitmap(TempSecondTypeFirstImagePath);
        else if (secondType1Exists)
            SecondTypeFirstImage = new Bitmap(FileStorage.GetFullPath(Path.Combine("Data", "ScheduleSecondType1.jpg")));
        else
            SecondTypeFirstImage = null;

        if (TempSecondTypeSecondImagePath is not null)
            SecondTypeSecondImage = new Bitmap(TempSecondTypeSecondImagePath);
        else if (secondType2Exists)
            SecondTypeSecondImage = new Bitmap(FileStorage.GetFullPath(Path.Combine("Data", "ScheduleSecondType2.jpg")));
        else
            SecondTypeSecondImage = null;

        ShowSecondTypeDefault = SecondTypeFirstImage is null && SecondTypeSecondImage is null;
        CanDelete = (IsFirstTypeSelected && firstTypeExists) || (IsSecondTypeSelected && (secondType1Exists || secondType2Exists));
        UpdateCanApply();
    }

    private async Task<string?> SelectImage(string title) {
        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (lifetime?.MainWindow?.StorageProvider is not { } storageProvider)
            return null;

        var options = new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] {
                new FilePickerFileType("Изображения") {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }
                }
            }
        };

        var result = await storageProvider.OpenFilePickerAsync(options);
        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    [RelayCommand]
    private async Task SelectFirstTypeImage() {
        TempFirstTypeImagePath = await SelectImage("Выбрать расписание");
        if (TempFirstTypeImagePath is not null) {
            FirstTypeImage = new Bitmap(TempFirstTypeImagePath);
            CanApply = true;
        }
    }

    [RelayCommand]
    private async Task SelectSecondTypeFirstImage() {
        TempSecondTypeFirstImagePath = await SelectImage("Выбрать первое расписание");
        if (TempSecondTypeFirstImagePath is not null) {
            SecondTypeFirstImage = new Bitmap(TempSecondTypeFirstImagePath);
            ShowSecondTypeDefault = false;
            UpdateCanApply();
        }
    }

    [RelayCommand]
    private async Task SelectSecondTypeSecondImage() {
        TempSecondTypeSecondImagePath = await SelectImage("Выбрать второе расписание");
        if (TempSecondTypeSecondImagePath is not null) {
            SecondTypeSecondImage = new Bitmap(TempSecondTypeSecondImagePath);
            ShowSecondTypeDefault = false;
            UpdateCanApply();
        }
    }

    private void UpdateCanApply() {
        CanApply = (IsFirstTypeSelected && TempFirstTypeImagePath is not null) ||
                   (IsSecondTypeSelected && (TempSecondTypeFirstImagePath is not null || TempSecondTypeSecondImagePath is not null));
    }

    [RelayCommand]
    private async Task Apply() {
        if (IsFirstTypeSelected && TempFirstTypeImagePath is not null) {
            using var stream = File.OpenRead(TempFirstTypeImagePath);
            await FileStorage.SaveStreamAsync(stream, Path.Combine("Data", "ScheduleFirstType.jpg"));
            TempFirstTypeImagePath = null;
        }
        else if (IsSecondTypeSelected) {
            if (TempSecondTypeFirstImagePath is not null) {
                using var stream = File.OpenRead(TempSecondTypeFirstImagePath);
                await FileStorage.SaveStreamAsync(stream, Path.Combine("Data", "ScheduleSecondType1.jpg"));
                TempSecondTypeFirstImagePath = null;
            }
            if (TempSecondTypeSecondImagePath is not null) {
                using var stream = File.OpenRead(TempSecondTypeSecondImagePath);
                await FileStorage.SaveStreamAsync(stream, Path.Combine("Data", "ScheduleSecondType2.jpg"));
                TempSecondTypeSecondImagePath = null;
            }
        }

        Settings.CustomSchedule = true;
        CanApply = false;
        CheckImagesExist();
    }

    [RelayCommand]
    private void Delete() {
        if (IsFirstTypeSelected) {
            FileStorage.RemoveFile(Path.Combine("Data", "ScheduleFirstType.jpg"));
            TempFirstTypeImagePath = null;
            FirstTypeImage = null;
        }
        else if (IsSecondTypeSelected) {
            FileStorage.RemoveFile(Path.Combine("Data", "ScheduleSecondType1.jpg"));
            FileStorage.RemoveFile(Path.Combine("Data", "ScheduleSecondType2.jpg"));
            TempSecondTypeFirstImagePath = null;
            TempSecondTypeSecondImagePath = null;
            SecondTypeFirstImage = null;
            SecondTypeSecondImage = null;
            ShowSecondTypeDefault = true;
        }

        CanApply = false;
        CheckImagesExist();
    }

    [RelayCommand]
    private void BackToAllSettings() {
        _parentViewModel.BackToAllSettings();
    }
}
