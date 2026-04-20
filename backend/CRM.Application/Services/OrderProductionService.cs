using AutoMapper;
using CRM.Application.DTOs.Production;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class OrderProductionService : IOrderProductionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IQrCodeService _qrCodeService;

    public OrderProductionService(IUnitOfWork unitOfWork, IMapper mapper, IQrCodeService qrCodeService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _qrCodeService = qrCodeService;
    }

    public async Task InitializeStepsAsync(Guid orderId)
    {
        var stages = await _unitOfWork.ProductionStages.GetActiveStagesOrderedAsync();
        await _unitOfWork.OrderProductionSteps.InitializeStepsForOrderAsync(orderId, stages);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<OrderProductionProgressDto> GetProgressAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        var steps = (await _unitOfWork.OrderProductionSteps.GetByOrderIdAsync(orderId)).ToList();
        return BuildProgressDto(order.Id, order.OrderNumber, null, (int)order.Status, steps);
    }

    public async Task<OrderProductionProgressDto> GetProgressByTokenAsync(string qrToken)
    {
        var orderId = _qrCodeService.DecodeToken(qrToken)
            ?? throw new KeyNotFoundException("QR code không hợp lệ.");

        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        var steps = (await _unitOfWork.OrderProductionSteps.GetByOrderIdAsync(orderId)).ToList();
        return BuildProgressDto(order.Id, order.OrderNumber, order.Customer?.Name, (int)order.Status, steps);
    }

    public async Task<OrderProductionStepDto> CompleteStepAsync(Guid orderId, Guid stageId, Guid userId, CompleteProductionStepDto dto)
    {
        var step = await _unitOfWork.OrderProductionSteps.GetByOrderAndStageAsync(orderId, stageId)
            ?? throw new KeyNotFoundException("Không tìm thấy bước sản xuất.");

        if (step.IsCompleted)
            throw new InvalidOperationException("Bước này đã được hoàn thành.");

        await EnsureUserAuthorizedForStageAsync(userId, step.ProductionStage);

        step.IsCompleted = true;
        step.CompletedByUserId = userId;
        step.CompletedAt = DateTime.UtcNow;
        step.Notes = dto.Notes?.Trim();

        _unitOfWork.OrderProductionSteps.Update(step);
        await _unitOfWork.SaveChangesAsync();

        // QC giờ là khâu 5 trong flow production (không phải giai đoạn riêng).
        // Khi xong hết tất cả 6 khâu (bao gồm QC + Đóng gói) → đơn hàng đã sẵn sàng giao.
        if (await _unitOfWork.OrderProductionSteps.AreAllStepsCompletedAsync(orderId))
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order != null && (order.Status == OrderStatus.InProduction || order.Status == OrderStatus.QualityCheck))
            {
                order.Status = OrderStatus.ReadyToShip;
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // Reload để có navigation properties
        var updated = await _unitOfWork.OrderProductionSteps.GetByOrderAndStageAsync(orderId, stageId);
        return _mapper.Map<OrderProductionStepDto>(updated!);
    }

    public async Task<OrderProductionStepDto> CompleteStepByTokenAsync(string qrToken, Guid stageId, Guid userId, CompleteProductionStepDto dto)
    {
        var orderId = _qrCodeService.DecodeToken(qrToken)
            ?? throw new KeyNotFoundException("QR code không hợp lệ.");
        return await CompleteStepAsync(orderId, stageId, userId, dto);
    }

    public async Task<IEnumerable<OrderProductionProgressDto>> GetAllInProductionAsync()
    {
        var orders = await _unitOfWork.Orders.GetOrdersByStatusAsync(Core.Enums.OrderStatus.InProduction, 100);
        var result = new List<OrderProductionProgressDto>();
        foreach (var order in orders)
        {
            var steps = (await _unitOfWork.OrderProductionSteps.GetByOrderIdAsync(order.Id)).ToList();
            result.Add(BuildProgressDto(order.Id, order.OrderNumber, order.Customer?.Name, (int)order.Status, steps));
        }
        return result;
    }

    // ---------------------------------------------------------------
    // Kiểm tra user có đúng role phụ trách khâu này không.
    // Override: Admin và ProductionManager có thể complete bất kỳ khâu nào.
    // Fallback: ProductionStaff (đa năng) có thể làm các khâu sản xuất (không phải QC).
    private async Task EnsureUserAuthorizedForStageAsync(Guid userId, ProductionStage? stage)
    {
        if (stage == null) return;
        var required = stage.ResponsibleRole;
        if (string.IsNullOrWhiteSpace(required)) return;

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId)
            ?? throw new UnauthorizedAccessException("Không xác định được người dùng.");

        var roles = user.UserRoles
            .Select(ur => ur.Role?.Name)
            .Where(n => !string.IsNullOrEmpty(n))
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Override cho cấp quản lý
        if (roles.Contains(RoleNames.Admin) || roles.Contains(RoleNames.ProductionManager))
            return;

        // Đúng role phụ trách
        if (roles.Contains(required))
            return;

        // ProductionStaff đa năng được phép làm các khâu sản xuất (trừ QC)
        var isQcStage = required.Equals(RoleNames.QualityControl, StringComparison.OrdinalIgnoreCase)
                     || required.Equals(RoleNames.QualityManager, StringComparison.OrdinalIgnoreCase);
        if (!isQcStage && roles.Contains(RoleNames.ProductionStaff))
            return;

        // QualityManager có thể làm khâu QC
        if (isQcStage && roles.Contains(RoleNames.QualityManager))
            return;

        throw new UnauthorizedAccessException(
            $"Bạn không có quyền hoàn thành khâu '{stage.StageName}'. Yêu cầu role: {required}.");
    }

    private static OrderProductionProgressDto BuildProgressDto(
        Guid orderId, string orderNumber, string? customerName, int orderStatus,
        List<Core.Entities.OrderProductionStep> steps)
    {
        var total = steps.Count;
        var completed = steps.Count(s => s.IsCompleted);
        var current = steps.FirstOrDefault(s => !s.IsCompleted);

        return new OrderProductionProgressDto
        {
            OrderId = orderId,
            OrderNumber = orderNumber,
            CustomerName = customerName,
            OrderStatus = orderStatus,
            TotalSteps = total,
            CompletedSteps = completed,
            ProgressPercent = total == 0 ? 0 : (int)Math.Round(completed * 100.0 / total),
            CurrentStageName = current?.ProductionStage?.StageName,
            IsFullyCompleted = total > 0 && completed == total,
            Steps = steps.Select(s => new OrderProductionStepDto
            {
                Id = s.Id,
                OrderId = s.OrderId,
                ProductionStageId = s.ProductionStageId,
                StageOrder = s.ProductionStage?.StageOrder ?? 0,
                StageName = s.ProductionStage?.StageName ?? string.Empty,
                ResponsibleRole = s.ProductionStage?.ResponsibleRole,
                IsCompleted = s.IsCompleted,
                CompletedByUserId = s.CompletedByUserId,
                CompletedByUserName = s.CompletedByUser != null
                    ? $"{s.CompletedByUser.FirstName} {s.CompletedByUser.LastName}"
                    : null,
                CompletedAt = s.CompletedAt,
                Notes = s.Notes
            }).ToList()
        };
    }
}
