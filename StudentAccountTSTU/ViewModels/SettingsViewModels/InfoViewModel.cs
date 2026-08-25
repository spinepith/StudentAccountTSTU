using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudentAccountTSTU.ViewModels.SettingsViewModels;

namespace StudentAccountTSTU.ViewModels;

internal partial class InfoViewModel : ViewModelBase {
    private readonly SettingsViewModel _parentViewModel;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _author;

    internal InfoViewModel(SettingsViewModel parentViewModel) {
        _parentViewModel = parentViewModel;

        _description =
            "Программа помогает автоматически получать ваши учебные данные из личного кабинета студента: баллы, расписание, рейтинг и т.д. "            +
            "Она работает так, как если бы вы сами открывали страницы и получали информацию - только делает это за вас.\n\n"                            +
            "Программа не взаимодействует напрямую с внутренними системами учебного заведения, поэтому процесс может занимать чуть больше времени.\n\n" +
            "Всё, что программа собирает, остается только на вашем устройстве. Никакие ваши данные никуда не отправляются.";

        _author = $"Версия {typeof(App).Assembly.GetName().Version?.ToString(3) ?? "1.0.0"}\nРазработано студентом группы БВТ231\nДубенский Василий";
    }

    [RelayCommand]
    private void BackToSettings() {
        _parentViewModel.BackToAllSettings();
    }
}
