using CRM.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using QRCoder;

namespace CRM.Application.Services;

public class QrCodeService : IQrCodeService
{
    private readonly string _frontendBaseUrl;

    public QrCodeService(IConfiguration configuration)
    {
        _frontendBaseUrl = configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:4200";
    }

    public string GenerateToken(Guid orderId)
    {
        // URL-safe Base64 of the 16-byte Guid → 22 chars, deterministic
        return Convert.ToBase64String(orderId.ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public Guid? DecodeToken(string token)
    {
        try
        {
            var padded = token.Replace("-", "+").Replace("_", "/");
            padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(padded);
            if (bytes.Length != 16) return null;
            return new Guid(bytes);
        }
        catch
        {
            return null;
        }
    }

    public Task<string> GenerateQrCodeBase64Async(Guid orderId, string orderNumber)
    {
        var token = GenerateToken(orderId);
        var url = $"{_frontendBaseUrl}/scan/{token}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(6); // 6 pixels per module

        return Task.FromResult(Convert.ToBase64String(pngBytes));
    }
}
