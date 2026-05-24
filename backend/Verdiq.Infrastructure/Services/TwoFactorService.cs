using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Auth;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public interface ITwoFactorService
{
    Task<TwoFactorSetupResponse> SetupAsync(Guid userId);
    Task<bool> VerifyAsync(Guid userId, string code);
    Task<bool> VerifyCodeOnlyAsync(string secret, string code);
    Task DisableAsync(Guid userId);
}

public class TwoFactorService : ITwoFactorService
{
    private readonly AppDbContext _context;

    public TwoFactorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TwoFactorSetupResponse> SetupAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var secret = GenerateSecret();
        var key = Base32Encode(secret);

        user.TwoFactorSecret = key;
        user.TwoFactorEnabled = true;
        await _context.SaveChangesAsync();

        var issuer = "Verdiq";
        var qrCodeUrl = $"otpauth://totp/{issuer}:{user.Email}?secret={key}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";

        return new TwoFactorSetupResponse
        {
            Secret = key,
            QrCodeUrl = qrCodeUrl,
            ManualEntryKey = key
        };
    }

    public async Task<bool> VerifyAsync(Guid userId, string code)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            return false;

        var isValid = ValidateTOTP(user.TwoFactorSecret, code);
        if (isValid)
        {
            user.TwoFactorVerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return isValid;
    }

    public Task<bool> VerifyCodeOnlyAsync(string secret, string code)
    {
        return Task.FromResult(ValidateTOTP(secret, code));
    }

    public async Task DisableAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        user.TwoFactorSecret = null;
        user.TwoFactorEnabled = false;
        user.TwoFactorVerifiedAt = null;
        await _context.SaveChangesAsync();
    }

    private static bool ValidateTOTP(string secret, string code)
    {
        try
        {
            var bytes = Base32Decode(secret);
            var currentTime = DateTime.UtcNow;
            var timestamps = new[]
            {
                GetTimestamp(currentTime),
                GetTimestamp(currentTime.AddSeconds(-30)),
                GetTimestamp(currentTime.AddSeconds(30))
            };

            return timestamps.Any(t => GenerateTOTP(bytes, t) == code);
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateTOTP(byte[] key, long timestamp)
    {
        var timeBytes = BitConverter.GetBytes(timestamp);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(timeBytes);
        var offset = hash[^1] & 0xf;
        var binary = (hash[offset] & 0x7f) << 24 |
                     (hash[offset + 1] & 0xff) << 16 |
                     (hash[offset + 2] & 0xff) << 8 |
                     (hash[offset + 3] & 0xff);
        var otp = binary % 1000000;
        return otp.ToString("D6");
    }

    private static long GetTimestamp(DateTime time)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(time - epoch).TotalSeconds / 30;
    }

    private static byte[] GenerateSecret()
    {
        var buffer = new byte[20];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);
        return buffer;
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new System.Text.StringBuilder();
        var bits = 0;
        var value = 0;

        foreach (var b in data)
        {
            value = (value << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                result.Append(alphabet[(value >> (bits - 5)) & 0x1f]);
                bits -= 5;
            }
        }

        if (bits > 0)
            result.Append(alphabet[(value << (5 - bits)) & 0x1f]);

        return result.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var cleaned = input.ToUpper().Replace(" ", "").TrimEnd('=');
        var bytes = new List<byte>();
        var bits = 0;
        var value = 0;

        foreach (var c in cleaned)
        {
            var idx = alphabet.IndexOf(c);
            if (idx < 0) continue;
            value = (value << 5) | idx;
            bits += 5;
            if (bits >= 8)
            {
                bytes.Add((byte)((value >> (bits - 8)) & 0xff));
                bits -= 8;
            }
        }
        return bytes.ToArray();
    }
}
