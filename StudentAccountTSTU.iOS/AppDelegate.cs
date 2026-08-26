using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Media;

using Foundation;
using PhotosUI;
using UIKit;

namespace StudentAccountTSTU.iOS {
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
            PlatformHooks.NativeGaleryAction = () => {
                var tcs = new System.Threading.Tasks.TaskCompletionSource<string?>();

                var config = new PHPickerConfiguration {
                    SelectionLimit = 1,
                    Filter         = PHPickerFilter.ImagesFilter
                };

                var picker = new PHPickerViewController(config);
                picker.Delegate = new ModernPhotoPickerDelegate(tcs);

                var windowScene = UIApplication.SharedApplication.ConnectedScenes
                .ToArray()
                .OfType<UIWindowScene>()
                .FirstOrDefault(s => s.ActivationState is UISceneActivationState.ForegroundActive);

                var window = windowScene?.Windows.FirstOrDefault(w => w.IsKeyWindow);
                var rootVC = window?.RootViewController;

                while (rootVC?.PresentedViewController is not null)
                    rootVC = rootVC.PresentedViewController;

                rootVC?.PresentViewController(picker, true, null);

                return tcs.Task;
            };
            
            return base.CustomizeAppBuilder(builder)
                .WithInterFont();
        }

        private class ModernPhotoPickerDelegate : PHPickerViewControllerDelegate {
            private readonly TaskCompletionSource<string?> _tcs;

            public ModernPhotoPickerDelegate(TaskCompletionSource<string?> tcs) {
                _tcs = tcs;
            }

            public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results) {
                picker.DismissViewController(true, null);

                if (results is null || results.Length is 0) {
                    _tcs.TrySetResult(null);
                    return;
                }

                var provider = results[0].ItemProvider;
                if (provider.HasItemConformingTo("public.image")) {
                    provider.LoadFileRepresentation("public.image", (url, error) => {
                        if (url?.Path is not null) {
                            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{System.Guid.NewGuid()}.img");
                            System.IO.File.Copy(url.Path, tempFile, true);
                            _tcs.TrySetResult(tempFile);
                        }
                        else
                            _tcs.TrySetResult(null);
                    });
                }
                else
                    _tcs.TrySetResult(null);
            }
        }
    }
}
