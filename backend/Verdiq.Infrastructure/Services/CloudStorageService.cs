using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Verdiq.Application.Interfaces;

namespace Verdiq.Infrastructure.Services;

public class CloudStorageService : ICloudStorageService
{
    private readonly string _provider;
    private readonly string _bucketName;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _region;
    private readonly string _localPath;
    private readonly bool _isEnabled;
    private readonly ILogger<CloudStorageService> _logger;

    public CloudStorageService(IConfiguration configuration, ILogger<CloudStorageService> logger)
    {
        _logger = logger;
        _provider = configuration["CloudStorage:Provider"] ?? "Local";
        _bucketName = configuration["CloudStorage:BucketName"] ?? "verdiq-documents";
        _accessKey = configuration["CloudStorage:AccessKey"] ?? string.Empty;
        _secretKey = configuration["CloudStorage:SecretKey"] ?? string.Empty;
        _region = configuration["CloudStorage:Region"] ?? "us-east-1";
        _localPath = configuration["DocumentStorage:Path"] ??
            Path.Combine(Directory.GetCurrentDirectory(), "Documents");
        _isEnabled = _provider != "Local" && !string.IsNullOrEmpty(_accessKey);

        if (!Directory.Exists(_localPath))
            Directory.CreateDirectory(_localPath);

        if (_isEnabled)
            _logger.LogInformation("Cloud storage enabled: {Provider} (bucket: {Bucket})", _provider, _bucketName);
        else
            _logger.LogInformation("Using local file storage at {Path}", _localPath);
    }

    public async Task<string> UploadAsync(string key, Stream fileStream, string contentType)
    {
        var storageKey = key;

        if (_isEnabled)
        {
            try
            {
                await UploadToS3Async(fileStream, storageKey, contentType);
                return storageKey;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "S3 upload failed, falling back to local storage");
            }
        }

        var filePath = Path.Combine(_localPath, storageKey);
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        await using (var fs = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        return storageKey;
    }

    public async Task<Stream?> DownloadAsync(string key)
    {
        if (_isEnabled)
        {
            try
            {
                return await DownloadFromS3Async(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "S3 download failed, checking local fallback");
            }
        }

        var filePath = Path.Combine(_localPath, key);
        if (!File.Exists(filePath))
            return null;

        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        if (_isEnabled)
        {
            try
            {
                await DeleteFromS3Async(key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "S3 delete failed, cleaning local");
            }
        }

        var filePath = Path.Combine(_localPath, key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }

    private Task UploadToS3Async(Stream fileStream, string key, string contentType)
    {
        throw new NotImplementedException("Install AWSSDK.S3 NuGet package to enable S3 uploads");
    }

    private Task<Stream> DownloadFromS3Async(string key)
    {
        throw new NotImplementedException("Install AWSSDK.S3 NuGet package to enable S3 downloads");
    }

    private Task DeleteFromS3Async(string key)
    {
        throw new NotImplementedException("Install AWSSDK.S3 NuGet package to enable S3 deletes");
    }
}
