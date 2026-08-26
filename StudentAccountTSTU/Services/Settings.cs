using System;
using System.IO;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;


namespace StudentAccountTSTU.Services;

public partial class Settings : ObservableObject {
    private static readonly string Path =
        OperatingSystem.IsBrowser() ? "Settings.json" :
        OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()
        ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Settings.json")
        : System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StudentAccountTSTU", "Settings.json");

    #region SETTINGS
    [ObservableProperty]
    private string? _baseURL = null;

    [ObservableProperty]
    private string? _deviceId = null;

    [ObservableProperty]
    private string? _userLogin = null;

    [ObservableProperty]
    private string? _userPassword = null;

    /* ОСНОВНЫЕ */
    [ObservableProperty]
    private bool _saveData = true;

    [ObservableProperty]
    private bool _customSchedule = false;

    /* ИНТЕРФЕЙС */
    [ObservableProperty]
    private bool _useBlur = OperatingSystem.IsWindows();

    [ObservableProperty]
    private string _accentColor = "#4DA1FF";

    [ObservableProperty]
    private bool _customAvatar = false;
    #endregion

    public Settings() {
        PropertyChanged += (s, e) => Save();
    }

    public static Settings Load() {
        try {
            if (OperatingSystem.IsBrowser()) {
                var json = BrowserStorage.GetLocalStorageItem(Path);
                if (!string.IsNullOrWhiteSpace(json)) {
                    var settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                    settings.PropertyChanged += (s, e) => settings.Save();
                    return settings;
                }
            }
            else if (File.Exists(Path)) {
                var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path)) ?? new Settings();
                settings.PropertyChanged += (s, e) => settings.Save();
                return settings;
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Error loading settings: {ex}");
        }

        var newSettings = new Settings();
        newSettings.Save();
        return newSettings;
    }

    private void Save() {
        try {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            
            if (OperatingSystem.IsBrowser())
                BrowserStorage.SetLocalStorageItem(Path, json);
            else {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
                File.WriteAllText(Path, json);
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving settings: {ex}");
        }
    }
}
