using Android.App;
using Android.Content.PM;

using Avalonia.Android;

namespace StudentAccountTSTU.Android {
    [Activity(
        Label = "Student Account TSTU",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity {
    }
}
