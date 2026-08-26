using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Platform;

namespace StudentAccountTSTU.AttachedProperties;

public class BlurProperty : AvaloniaObject {
    public static readonly AttachedProperty<bool> BlurAttachedProperty =
        AvaloniaProperty.RegisterAttached<BlurProperty, Window, bool>(
            "BlurAttached",
            defaultValue: false,
            inherits: false,
            defaultBindingMode: BindingMode.OneWay,
            coerce: (obj, value) => {
                if (obj is Window window) {
                    ApplyBlur(window, (bool)value);
                }
                return value;
            });

    static BlurProperty() {
        BlurAttachedProperty.Changed.AddClassHandler<Window>((window, e) => {
            ApplyBlur(window, (bool)e.NewValue!);
        });
    }

    public static void SetBlurAttached(Window element, bool value) {
        element.SetValue(BlurAttachedProperty, value);
    }

    public static bool GetBlurAttached(Window element) {
        return element.GetValue(BlurAttachedProperty);
    }

    private static void OnBlurChanged(Window window, AvaloniaPropertyChangedEventArgs e) {
        ApplyBlur(window, (bool)e.NewValue!);
    }

    private static void ApplyBlur(Window window, bool useBlur) {
        if (useBlur && OperatingSystem.IsWindows()) {
            window.TransparencyLevelHint = new[] {
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.Blur
            };
            window.Background = Brushes.Transparent;
        }
        else {
            window.TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
            window.Background = new SolidColorBrush(Color.Parse("#FF282C34"));
        }
    }
}
