using System;
using System.IO;
using System.Reflection;
using System.Text.Json;


namespace StudentAccountTSTU.Services;

public class Settings {
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Assembly.GetEntryAssembly()!.GetName().Name!,
        "settings.json"
    );

    #region SETTINGS
    public string? BaseURL       { get; set; } = null;

    public string? DeviceId      { get; set; } = null;
    public string? UserLogin     { get; set; } = null;
    public string? UserPassword  { get; set; } = null;

    public bool SaveData         { get; set; } = true;

    public bool UserBlur         { get; set; } = true;

    public string SelectionColor { get; set; } = "#4DA1FF";
    #endregion

    public static Settings Load() {
        if (File.Exists(Path)) {
            try {
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path)) ?? new Settings();
            }
            catch {
                return new Settings();
            }
        }
        return new Settings();
    }

    public void Save() {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        File.WriteAllText(Path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
