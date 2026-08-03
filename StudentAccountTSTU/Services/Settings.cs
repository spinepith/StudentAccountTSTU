using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;


namespace StudentAccountTSTU.Services;

public partial class Settings : ObservableObject {
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "StudentAccountTSTU",
        "Settings.json"
    );

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
    private bool _useBlur = true;

    [ObservableProperty]
    private string _accentColor = "#4DA1FF";

    [ObservableProperty]
    private bool _customAvatar = false;
    #endregion

    public Settings() {
        PropertyChanged += (s, e) => Save();
    }

    public static Settings Load() {
        if (File.Exists(Path)) {
            try {
                var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path)) ?? new Settings();
                settings.PropertyChanged += (s, e) => settings.Save();
                return settings;
            }
            catch {
                return new Settings();
            }
        }
        return new Settings();
    }

    private void Save() {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        File.WriteAllText(Path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
