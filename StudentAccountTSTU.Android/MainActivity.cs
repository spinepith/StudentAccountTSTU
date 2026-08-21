using Android.App;
using Android.Content.PM;
using Android.OS;

using Avalonia.Android;

namespace StudentAccountTSTU.Android {
    [Activity(
        Label = "Student Account TSTU",
        Theme = "@style/MyTheme.NoActionBar",
        Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            StudentAccountTSTU.PlatformHooks.NativeGaleryAction = async () => {
                _pickImageTcs = TaskCompletionSource<string?>();

                var intent = new Intent(Intent.ActionPick, Android.Provider.MediaStore.Images.Media.ExternalContentUri);
                intent.SetType("image/*");

                StartActivityForResult(intent, PickImageRequestCode);

                return _pickImageTcs.Task;
            };
        }
        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            if (requestCode is PickImageCode) {
                try {
                    var uri = data.Data;
                    string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{System.Guid.NewGuid()}.img");

                    using (var stream = ContentResolver?.OpenInputStream(uri));
                    using (var outStream = System.IO.File.Create(tempFile))
                        stream?.CopyTo(outStream);

                    _pickImageTcs?.TrySetResult(tempFile);
                }
                catch {
                    _pickImageTcs?.TrySetResult(null);
                }
            }
            else {
                _pickImageTcs = null;
                return;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}
