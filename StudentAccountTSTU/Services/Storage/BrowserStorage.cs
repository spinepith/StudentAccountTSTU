using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;

using StudentAccountTSTU.Services.Storage;

namespace StudentAccountTSTU.Services;

internal partial class BrowserStorage : IStorage {
    public string GetFullPath(string path) {
        return path;
    }

    public async Task<bool> CheckExistsAsync(string path) {
        return await CheckExistsJSAsync(path);
    }

    public async Task<string[]> GetFilesAsync(string path) {
        try {
            var keysStr = await GetAllKeysJSAsync();
            if (string.IsNullOrEmpty(keysStr)) return Array.Empty<string>();

            var prefix = path.Replace('\\', '/');
            if (!prefix.EndsWith('/'))
                prefix += '/';

            var result = new System.Collections.Generic.List<string>();
            var keys = keysStr.Split(',');
            foreach (var key in keys)
                if (!string.IsNullOrEmpty(key) && key.StartsWith(prefix))
                    result.Add(key);

            return result.ToArray();
        }
        catch { }
        return Array.Empty<string>();
    }

    public async Task<DateTime?> GetLastModifiedAsync(string path) {
        var ms = await GetLastModifiedJSAsync(path);
        if (ms < 0)
            return null;
        return DateTime.UnixEpoch.AddMilliseconds(ms).ToLocalTime();
    }

    public async Task<T?> GetAsync<T>(string path) {
        var json = await GetItemAsync(path);
        if (string.IsNullOrEmpty(json))
            return default;
        try {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch {
            return default;
        }
    }

    public async Task<Stream> GetFileStreamAsync(string path) {
        try {
            var base64 = await GetItemAsync(path);
            if (!string.IsNullOrEmpty(base64)) {
                var bytes = Convert.FromBase64String(base64);
                return new MemoryStream(bytes);
            }
        }
        catch { }
        return Stream.Null;
    }

    public async Task SaveAsync<T>(T data, string path) {
        var json = JsonSerializer.Serialize(data);
        await SetItemAsync(path, json);
    }

    public async Task SaveStreamAsync(Stream stream, string path) {
        try {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());
            await SetItemAsync(path, base64);
        }
        catch { }
    }

    public async Task RemoveFileAsync(string path) {
        await RemoveFileJSAsync(path);
    }

    public async Task RemoveDirectoryAsync(string path) {
        await RemoveDirectoryJSAsync(path);
    }

    // FILE STORAGE
    [JSImport("checkExists", "app_storage.js")]
    private static partial Task<bool> CheckExistsJSAsync(string key);

    [JSImport("getAllKeys", "app_storage.js")]
    private static partial Task<string> GetAllKeysJSAsync();

    [JSImport("getLastModified", "app_storage.js")]
    private static partial Task<double> GetLastModifiedJSAsync(string key);

    [JSImport("getItem", "app_storage.js")]
    private static partial Task<string?> GetItemAsync(string key);

    [JSImport("setItem", "app_storage.js")]
    private static partial Task SetItemAsync(string key, string value);

    [JSImport("removeFile", "app_storage.js")]
    private static partial Task RemoveFileJSAsync(string key);

    [JSImport("removeDirectory", "app_storage.js")]
    private static partial Task RemoveDirectoryJSAsync(string prefix);


    // SETTINGS

    [JSImport("getLocalStorageItem", "app_storage.js")]
    public static partial string? GetLocalStorageItem(string key);

    [JSImport("setLocalStorageItem", "app_storage.js")]
    public static partial void SetLocalStorageItem(string key, string value);
}
