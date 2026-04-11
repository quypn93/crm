using CRM.Application.DTOs.Production;

namespace CRM.Application.Interfaces;

public interface IOrderProductionService
{
    Task<OrderProductionProgressDto> GetProgressAsync(Guid orderId);
    Task<OrderProductionProgressDto> GetProgressByTokenAsync(string qrToken);
    Task<OrderProductionStepDto> CompleteStepAsync(Guid orderId, Guid stageId, Guid userId, CompleteProductionStepDto dto);
    Task<OrderProductionStepDto> CompleteStepByTokenAsync(string qrToken, Guid stageId, Guid userId, CompleteProductionStepDto dto);
    Task InitializeStepsAsync(Guid orderId);
    Task<IEnumerable<OrderProductionProgressDto>> GetAllInProductionAsync();
}
