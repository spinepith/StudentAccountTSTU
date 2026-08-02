using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace StudentAccountTSTU.AttachedProperties;

public class AccentColorProperty : AvaloniaObject {
    public static readonly AttachedProperty<string> AccentColorAttachedProperty =
        AvaloniaProperty.RegisterAttached<AccentColorProperty, Window, string>(
            "AccentColorAttached",
            defaultValue: "#4DA1FF",
            inherits: false,
            defaultBindingMode: BindingMode.OneWay);

    static AccentColorProperty() {
        AccentColorAttachedProperty.Changed.AddClassHandler<Window>(OnAccentColorChanged);
    }

    public static void SetAccentColorAttached(Window element, string value) {
        element.SetValue(AccentColorAttachedProperty, value);
    }

    public static string GetAccentColorAttached(Window element) {
        return element.GetValue(AccentColorAttachedProperty);
    }

    private static void OnAccentColorChanged(Window window, AvaloniaPropertyChangedEventArgs e) {
        var colorString = (string)e.NewValue!;

        try {
            var color = Color.Parse(colorString);

            if (Application.Current!.Resources.ContainsKey("AccentColor"))
                Application.Current.Resources["AccentColor"] = color;
            else
                Application.Current.Resources.Add("AccentColor", color);
        }
        catch { }
    }
}
