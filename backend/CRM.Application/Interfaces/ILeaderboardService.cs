using CRM.Application.DTOs.Report;

namespace CRM.Application.Interfaces;

public interface ILeaderboardService
{
    Task<LeaderboardResultDto> GetLeaderboardAsync(LeaderboardScope scope, LeaderboardPeriod period, DateTime referenceDate);
}
