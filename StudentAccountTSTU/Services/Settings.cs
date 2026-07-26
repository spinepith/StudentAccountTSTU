using System;
using System.IO;
using System.Text.Json;


namespace StudentAccountTSTU.Services;

internal class Settings {
    private static readonly string Path = System.IO.Path.Combine(AppContext.BaseDirectory, "settings.json");

    #region SETTINGS
    public string BaseURL  { get; set; } = string.Empty;
    public bool SaveData   { get; set; } = true;
    #endregion

    public static Settings Load() => File.Exists(Path) ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path)) ?? new() : new();
    internal void Save() => File.WriteAllText(Path, JsonSerializer.Serialize(this));
}
