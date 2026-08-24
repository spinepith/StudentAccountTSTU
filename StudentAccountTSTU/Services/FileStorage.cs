using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;


namespace StudentAccountTSTU.Services;

internal static class FileStorage {
    private static readonly string AppDirectory =
        OperatingSystem.IsBrowser() ? "" : OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()
            ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StudentAccountTSTU");

    public static string GetFullPath(string path) {
        if (OperatingSystem.IsBrowser())
            ;
        return Path.Combine(AppDirectory, path);
    }

    public static bool CheckExists(string path) {
        if (OperatingSystem.IsBrowser())
            ;
        return File.Exists(Path.Combine(AppDirectory, path));
    }

    public static async Task<T?> GetAsync<T>(string path) {
        try {
            if (OperatingSystem.IsBrowser())
                ;
            else {
                if (File.Exists(Path.Combine(AppDirectory, path))) {
                    using FileStream stream = File.OpenRead(Path.Combine(AppDirectory, path));
                    return await JsonSerializer.DeserializeAsync<T>(stream);
                }
            }
        }
        catch { }
        return default;
    }

    public static async Task SaveAsync<T>(T data, string path) {
        var settings = App.Services.GetRequiredService<Settings>();
        if (!settings.SaveData)
            return;

        try {
            if (OperatingSystem.IsBrowser())
                ;
            else {
                var fullPath = Path.Combine(AppDirectory, path);
                var directory = Path.GetDirectoryName(fullPath);
                if (directory is not null)
                    Directory.CreateDirectory(directory);

                using FileStream stream = File.Create(fullPath);
                await JsonSerializer.SerializeAsync(stream, data);
            }
        }
        catch { }
    }

    public static async Task SaveStreamAsync(Stream stream, string path) {
        var settings = App.Services.GetRequiredService<Settings>();
        if (!settings.SaveData)
            return;

        try {
            if (OperatingSystem.IsBrowser())
                ;
            else {
                var fullPath = Path.Combine(AppDirectory, path);
                var directory = Path.GetDirectoryName(fullPath);
                if (directory is not null)
                    Directory.CreateDirectory(directory);

                using var outputStream = File.Create(fullPath);
                await stream.CopyToAsync(outputStream);
                await outputStream.FlushAsync();
            }
        }
        catch { }
    }

    public static Stream GetFileStreamAsync(string path) {
        if (OperatingSystem.IsBrowser())
            ;
        return File.OpenRead(Path.Combine(AppDirectory, path));
    }

    public static void RemoveFile(string path) {
        try {
            if (OperatingSystem.IsBrowser())
                ;
            else {
                path = Path.Combine(AppDirectory, path);
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        catch { }
    }

    public static void RemoveDirectory(string path) {
        try {
            if (OperatingSystem.IsBrowser())
                ;
            else {
                path = Path.Combine(AppDirectory, path);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
        }
        catch { }
    }

    public static DateTime? GetLastModified(string path) {
        try {
            if (OperatingSystem.IsBrowser())
                ;
            else {
                var fullPath = Path.Combine(AppDirectory, path);
                if (File.Exists(fullPath))
                    return File.GetLastWriteTime(fullPath);
            }
        }
        catch { }
        return null;
    }
}
