namespace CRM.Application.Interfaces;

public interface IQrCodeService
{
    string GenerateToken(Guid orderId);
    Guid? DecodeToken(string token);
    Task<string> GenerateQrCodeBase64Async(Guid orderId, string orderNumber);
}
