namespace Verdiq.Application.Interfaces;

public class CloudStorageResult
{
    public string StorageProvider { get; set; } = "Local";
    public string StorageKey { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

public interface ICloudStorageService
{
    Task<CloudStorageResult> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadAsync(string storageKey);
    Task DeleteAsync(string storageKey);
    Task<string> GenerateSignedUrlAsync(string storageKey, int expiresInMinutes = 60);
    bool IsEnabled { get; }
}
