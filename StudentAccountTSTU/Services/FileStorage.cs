using System;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;


namespace StudentAccountTSTU.Services;

internal static class FileStorage {
    private static readonly string AppDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static async Task<T?> Get<T>(string path) {
        if (File.Exists(Path.Combine(AppDirectory, path))) {
            using FileStream stream = File.OpenRead(Path.Combine(AppDirectory, path));
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
        return default;
    }

    public static async void Save<T>(T data, string path) {
        using FileStream stream = File.Create(Path.Combine(AppDirectory, path));
        await JsonSerializer.SerializeAsync(stream, data);
    }
}
