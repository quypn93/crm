using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Deal;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Application.Interfaces;

namespace CRM.Application.Services;

public class DealService : IDealService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DealService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DealDto?> GetByIdAsync(Guid id)
    {
        var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(id);
        return deal != null ? _mapper.Map<DealDto>(deal) : null;
    }

    public async Task<PaginatedResult<DealDto>> GetPagedAsync(DealFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.Deals.GetPagedAsync(
            filter.Search,
            filter.StageId,
            filter.CustomerId,
            filter.AssignedTo,
            filter.MinValue,
            filter.MaxValue,
            filter.CloseDateFrom,
            filter.CloseDateTo,
            filter.Page,
            filter.PageSize,
            filter.SortBy,
            filter.SortOrder);

        var dtos = _mapper.Map<List<DealDto>>(items);
        return PaginatedResult<DealDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<DealDto> CreateAsync(CreateDealDto dto, Guid userId)
    {
        var deal = _mapper.Map<Deal>(dto);
        deal.CreatedByUserId = userId;

        // If no stage specified, use default stage
        if (!dto.StageId.HasValue)
        {
            var defaultStage = await _unitOfWork.Deals.GetDefaultStageAsync();
            if (defaultStage != null)
            {
                deal.StageId = defaultStage.Id;
                deal.Probability = defaultStage.Probability;
            }
        }
        else
        {
            deal.StageId = dto.StageId.Value;
        }

        // If no assigned user specified, assign to creator
        if (!dto.AssignedToUserId.HasValue)
        {
            deal.AssignedToUserId = userId;
        }

        await _unitOfWork.Deals.AddAsync(deal);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(deal.Id) ?? throw new InvalidOperationException("Không thể tạo giao dịch.");
    }

    public async Task<DealDto> UpdateAsync(UpdateDealDto dto, Guid userId)
    {
        var deal = await _unitOfWork.Deals.GetByIdAsync(dto.Id);

        if (deal == null)
        {
            throw new KeyNotFoundException("Không tìm thấy giao dịch.");
        }

        _mapper.Map(dto, deal);
        _unitOfWork.Deals.Update(deal);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(deal.Id) ?? throw new InvalidOperationException("Không thể cập nhật giao dịch.");
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var deal = await _unitOfWork.Deals.GetByIdAsync(id);

        if (deal == null)
        {
            throw new KeyNotFoundException("Không tìm thấy giao dịch.");
        }

        _unitOfWork.Deals.Remove(deal);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DealDto> UpdateStageAsync(UpdateDealStageDto dto, Guid userId)
    {
        var deal = await _unitOfWork.Deals.GetByIdWithDetailsAsync(dto.DealId);

        if (deal == null)
        {
            throw new KeyNotFoundException("Không tìm thấy giao dịch.");
        }

        deal.StageId = dto.StageId;

        // Update probability based on new stage
        var stages = await _unitOfWork.Deals.GetAllStagesAsync();
        var newStage = stages.FirstOrDefault(s => s.Id == dto.StageId);
        if (newStage != null)
        {
            deal.Probability = newStage.Probability;
        }

        _unitOfWork.Deals.Update(deal);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(deal.Id) ?? throw new InvalidOperationException("Không thể cập nhật giao dịch.");
    }

    public async Task<DealDto> MarkAsWonAsync(Guid dealId, MarkDealWonDto dto, Guid userId)
    {
        var deal = await _unitOfWork.Deals.GetByIdAsync(dealId);

        if (deal == null)
        {
            throw new KeyNotFoundException("Không tìm thấy giao dịch.");
        }

        var wonStage = await _unitOfWork.Deals.GetWonStageAsync();
        if (wonStage == null)
        {
            throw new InvalidOperationException("Không tìm thấy giai đoạn Thắng.");
        }

        deal.StageId = wonStage.Id;
        deal.Probability = 100;
        deal.ActualCloseDate = dto.ActualCloseDate ?? DateTime.UtcNow;

        _unitOfWork.Deals.Update(deal);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(deal.Id) ?? throw new InvalidOperationException("Không thể cập nhật giao dịch.");
    }

    public async Task<DealDto> MarkAsLostAsync(Guid dealId, MarkDealLostDto dto, Guid userId)
    {
        var deal = await _unitOfWork.Deals.GetByIdAsync(dealId);

        if (deal == null)
        {
            throw new KeyNotFoundException("Không tìm thấy giao dịch.");
        }

        var lostStage = await _unitOfWork.Deals.GetLostStageAsync();
        if (lostStage == null)
        {
            throw new InvalidOperationException("Không tìm thấy giai đoạn Thua.");
        }

        deal.StageId = lostStage.Id;
        deal.Probability = 0;
        deal.LostReason = dto.LostReason;
        deal.ActualCloseDate = DateTime.UtcNow;

        _unitOfWork.Deals.Update(deal);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(deal.Id) ?? throw new InvalidOperationException("Không thể cập nhật giao dịch.");
    }

    public async Task<IEnumerable<DealStageDto>> GetAllStagesAsync()
    {
        var stages = await _unitOfWork.Deals.GetAllStagesAsync();
        return _mapper.Map<IEnumerable<DealStageDto>>(stages);
    }

    public async Task<IEnumerable<DealsByStageDto>> GetDealsByStageAsync()
    {
        var stagesWithDeals = await _unitOfWork.Deals.GetDealsByStageAsync();
        var result = new List<DealsByStageDto>();

        foreach (var (stage, count, totalValue) in stagesWithDeals)
        {
            var deals = await _unitOfWork.Deals.GetByStageAsync(stage.Id);
            result.Add(new DealsByStageDto
            {
                Stage = _mapper.Map<DealStageDto>(stage),
                Deals = _mapper.Map<List<DealDto>>(deals),
                Count = count,
                TotalValue = totalValue
            });
        }

        return result;
    }

    public async Task<IEnumerable<DealDto>> GetByCustomerAsync(Guid customerId)
    {
        var deals = await _unitOfWork.Deals.GetByCustomerAsync(customerId);
        return _mapper.Map<IEnumerable<DealDto>>(deals);
    }
}
