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

    public bool IsEnabled => _isEnabled;

    public async Task<CloudStorageResult> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var key = $"{Guid.NewGuid():N}_{fileName}";

        if (_isEnabled)
        {
            try
            {
                var result = await UploadToS3Async(fileStream, key, contentType);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "S3 upload failed, falling back to local storage");
            }
        }

        var filePath = Path.Combine(_localPath, key);
        await using (var fs = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        return new CloudStorageResult
        {
            StorageProvider = "Local",
            StorageKey = key,
            FilePath = filePath
        };
    }

    public async Task<Stream> DownloadAsync(string storageKey)
    {
        if (_isEnabled)
        {
            try
            {
                return await DownloadFromS3Async(storageKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "S3 download failed, checking local fallback");
            }
        }

        var filePath = Path.Combine(_localPath, storageKey);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Document file not found");

        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public async Task DeleteAsync(string storageKey)
    {
        if (_isEnabled)
        {
            try
            {
                await DeleteFromS3Async(storageKey);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "S3 delete failed, cleaning local");
            }
        }

        var filePath = Path.Combine(_localPath, storageKey);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public async Task<string> GenerateSignedUrlAsync(string storageKey, int expiresInMinutes = 60)
    {
        if (_isEnabled)
        {
            try
            {
                return await GenerateS3SignedUrlAsync(storageKey, expiresInMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "S3 signed URL generation failed");
            }
        }

        return await Task.FromResult(string.Empty);
    }

    private Task<CloudStorageResult> UploadToS3Async(Stream fileStream, string key, string contentType)
    {
        // AWS SDK S3 upload — requires AWSSDK.S3 NuGet package
        // using var s3Client = new AmazonS3Client(_accessKey, _secretKey, RegionEndpoint.GetBySystemName(_region));
        // var putRequest = new PutObjectRequest
        // {
        //     InputStream = fileStream,
        //     BucketName = _bucketName,
        //     Key = key,
        //     ContentType = contentType
        // };
        // await s3Client.PutObjectAsync(putRequest);
        // return new CloudStorageResult
        // {
        //     StorageProvider = "S3",
        //     StorageKey = key,
        //     FilePath = $"s3://{_bucketName}/{key}"
        // };

        throw new NotImplementedException("Install AWSSDK.S3 NuGet package to enable S3 uploads");
    }

    private Task<Stream> DownloadFromS3Async(string storageKey)
    {
        throw new NotImplementedException("Install AWSSDK.S3 NuGet package to enable S3 downloads");
    }

    private Task DeleteFromS3Async(string storageKey)
    {
        throw new NotImplementedException("Install AWSSDK.S3 NuGet package to enable S3 deletes");
    }

    private Task<string> GenerateS3SignedUrlAsync(string storageKey, int expiresInMinutes)
    {
        throw new NotImplementedException("Install AWSSDK.S3 NuGet package to enable S3 signed URLs");
    }
}
