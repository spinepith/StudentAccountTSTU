using System;
using System.IO;
using System.Threading.Tasks;

namespace StudentAccountTSTU.Services.Storage;

internal interface IStorage {
    string GetFullPath(string path);
    Task<bool> CheckExistsAsync(string path);
    Task<string[]> GetFilesAsync(string path);
    Task<DateTime?> GetLastModifiedAsync(string path);

    Task<T?> GetAsync<T>(string path);
    Task<Stream> GetFileStreamAsync(string path);
    
    Task SaveAsync<T>(T data, string path);
    Task SaveStreamAsync(Stream stream, string path);
    
    Task RemoveFileAsync(string path);
    Task RemoveDirectoryAsync(string path);    
}

internal static class FileStorage {
    private static readonly IStorage _storage;

    static FileStorage() {
        if (OperatingSystem.IsBrowser())
            _storage = new BrowserStorage();
        else
            _storage = new LocalFileStorage();
    }

    public static string GetFullPath(string path)                   => _storage.GetFullPath(path);
    public static Task<bool> CheckExistsAsync(string path)          => _storage.CheckExistsAsync(path);
    public static Task<T?> GetAsync<T>(string path)                 => _storage.GetAsync<T>(path);
    public static Task SaveAsync<T>(T data, string path)            => _storage.SaveAsync(data, path);
    public static Task SaveStreamAsync(Stream stream, string path)  => _storage.SaveStreamAsync(stream, path);
    public static Task<Stream> GetFileStreamAsync(string path)      => _storage.GetFileStreamAsync(path);
    public static Task RemoveFileAsync(string path)                 => _storage.RemoveFileAsync(path);
    public static Task RemoveDirectoryAsync(string path)            => _storage.RemoveDirectoryAsync(path);
    public static Task<DateTime?> GetLastModifiedAsync(string path) => _storage.GetLastModifiedAsync(path);
    public static Task<string[]> GetFilesAsync(string path)         => _storage.GetFilesAsync(path);
}
