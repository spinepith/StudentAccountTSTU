using Avalonia.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;


namespace StudentAccountTSTU.ViewModels;

internal partial class HomeViewModel : ViewModelBase {
    [ObservableProperty]
    private Bitmap? _userImage;

    [ObservableProperty]
    private bool _isLoading = true;
}
