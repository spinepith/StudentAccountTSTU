using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace StudentAccountTSTU.Services.Storage;

internal class LocalFileStorage : IStorage {
    private static readonly string AppDirectory =
        OperatingSystem.IsAndroid() || OperatingSystem.IsIOS()
        ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StudentAccountTSTU");

    public string GetFullPath(string path) {
        return Path.Combine(AppDirectory, path);
    }

    public Task<bool> CheckExistsAsync(string path) {
        return Task.Run(() => File.Exists(Path.Combine(AppDirectory, path)));
    }

    public Task<string[]> GetFilesAsync(string path) {
        return Task.Run(() => {
            try {
                var fullPath = Path.Combine(AppDirectory, path);
                if (Directory.Exists(fullPath)) {
                    var files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
                    var relativeFiles = new string[files.Length];
                    for (int i = 0; i < files.Length; i++) {
                        relativeFiles[i] = Path.GetRelativePath(AppDirectory, files[i]).Replace('\\', '/');
                    }
                    return relativeFiles;
                }
            }
            catch { }
            return Array.Empty<string>();
        });
    }

    public Task<DateTime?> GetLastModifiedAsync(string path) {
        return Task.Run(() => {
            try {
                var fullPath = Path.Combine(AppDirectory, path);
                if (File.Exists(fullPath))
                    return (DateTime?)File.GetLastWriteTime(fullPath);
            }
            catch { }
            return (DateTime?)null;
        });
    }

    public async Task<T?> GetAsync<T>(string path) {
        try {
            if (File.Exists(Path.Combine(AppDirectory, path))) {
                using FileStream stream = File.OpenRead(Path.Combine(AppDirectory, path));
                return await JsonSerializer.DeserializeAsync<T>(stream);
            }
        }
        catch { }
        return default;
    }

    public Task<Stream> GetFileStreamAsync(string path) {
        return Task.Run<Stream>(() => {
            try {
                return File.OpenRead(Path.Combine(AppDirectory, path));
            }
            catch { }
            return Stream.Null;
        });
    }

    public async Task SaveAsync<T>(T data, string path) {
        var settings = App.Services.GetRequiredService<Settings>();
        if (!settings.SaveData)
            return;

        try {
            var fullPath = Path.Combine(AppDirectory, path);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory is not null)
                Directory.CreateDirectory(directory);

            using FileStream stream = File.Create(fullPath);
            await JsonSerializer.SerializeAsync(stream, data);
        }
        catch { }
    }

    public async Task SaveStreamAsync(Stream stream, string path) {
        var settings = App.Services.GetRequiredService<Settings>();
        if (!settings.SaveData)
            return;

        try {
            var fullPath = Path.Combine(AppDirectory, path);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory is not null)
                Directory.CreateDirectory(directory);

            using var outputStream = File.Create(fullPath);
            await stream.CopyToAsync(outputStream);
            await outputStream.FlushAsync();
        }
        catch { }
    }

    public Task RemoveFileAsync(string path) {
        return Task.Run(() => {
            try {
                var fullPath = Path.Combine(AppDirectory, path);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch { }
        });
    }

    public Task RemoveDirectoryAsync(string path) {
        return Task.Run(() => {
            try {
                var fullPath = Path.Combine(AppDirectory, path);
                if (Directory.Exists(fullPath))
                    Directory.Delete(fullPath, true);
            }
            catch { }
        });
    }
}
