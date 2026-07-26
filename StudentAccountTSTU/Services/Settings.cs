using System;
using System.IO;
using System.Text.Json;


namespace StudentAccountTSTU.Services;

internal class Settings {
    private static readonly string Path = System.IO.Path.Combine(AppContext.BaseDirectory, "settings.json");

    #region SETTINGS
    public string? BaseURL      { get; set; } = null;
    public string? DeviceId     { get; set; } = null;
    public string? UserLogin    { get; set; } = null;
    public string? UserPassword { get; set; } = null;
    public bool SaveData        { get; set; } = true;
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

    internal void Save() => File.WriteAllText(Path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
}
