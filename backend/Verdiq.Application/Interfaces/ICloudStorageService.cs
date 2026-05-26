namespace Verdiq.Application.Interfaces;

public interface ICloudStorageService
{
    Task<string> UploadAsync(string key, Stream fileStream, string contentType);
    Task<Stream?> DownloadAsync(string key);
    Task<bool> DeleteAsync(string key);
}
