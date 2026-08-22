using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudentAccountTSTU.ViewModels;

internal partial class InfoViewModel : ViewModelBase {
    private readonly HomeViewModel _parentViewModel;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _author;

    internal InfoViewModel(HomeViewModel parentViewModel) {
        _parentViewModel = parentViewModel;

        _description =
            "Программа помогает автоматически получать ваши учебные данные из личного кабинета студента: баллы, расписание, рейтинг и т.д. "            +
            "Она работает так, как если бы вы сами открывали страницы и получали информацию - только делает это за вас.\n\n"                            +
            "Программа не взаимодействует напрямую с внутренними системами учебного заведения, поэтому процесс может занимать чуть больше времени.\n\n" +
            "Всё, что программа собирает, остается только на вашем устройстве. Никакие ваши данные никуда не отправляются.";

        _author = "Версия 0.0.2a\nРазработано студентом группы БВТ231\nДубенский Василий";
    }

    [RelayCommand]
    private void BackToHome() {
        _parentViewModel.BackToHome();
    }
}
