using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.Services;

namespace StudentAccountTSTU.ViewModels.SettingsViewModels;

internal partial class AccentColorViewModel : ViewModelBase {
    private readonly SettingsViewModel _parentViewModel;

    [ObservableProperty]
    private Settings _settings;

    [ObservableProperty]
    private string _colorInput;

    internal AccentColorViewModel(SettingsViewModel parentViewModel, Settings settings) {
        _parentViewModel = parentViewModel;
        Settings = settings;
        ColorInput = settings.AccentColor;
    }

    [RelayCommand]
    private void Apply() {
        Settings.AccentColor = ColorInput;
    }

    [RelayCommand]
    private void BackToAllSettings() {
        _parentViewModel.BackToAllSettings();
    }
}
