using AutoMapper;
using CRM.Application.DTOs.Auth;
using CRM.Application.DTOs.Customer;
using CRM.Application.DTOs.Deal;
using CRM.Application.DTOs.Task;
using CRM.Application.DTOs.Report;
using CRM.Core.Entities;

namespace CRM.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<RegisterRequestDto, User>()
            .ForMember(d => d.PasswordHash, opt => opt.Ignore());

        // Customer mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.CreatedByUserName, opt => opt.MapFrom(s => s.CreatedByUser != null ? $"{s.CreatedByUser.FirstName} {s.CreatedByUser.LastName}" : null))
            .ForMember(d => d.AssignedToUserName, opt => opt.MapFrom(s => s.AssignedToUser != null ? $"{s.AssignedToUser.FirstName} {s.AssignedToUser.LastName}" : null))
            .ForMember(d => d.DealsCount, opt => opt.MapFrom(s => s.Deals.Count))
            .ForMember(d => d.TasksCount, opt => opt.MapFrom(s => s.Tasks.Count))
            .ForMember(d => d.TotalDealsValue, opt => opt.MapFrom(s => s.Deals.Sum(d => d.Value)));

        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();

        // DealStage mappings
        CreateMap<DealStage, DealStageDto>();

        // Deal mappings
        CreateMap<Deal, DealDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer != null ? s.Customer.Name : null))
            .ForMember(d => d.StageName, opt => opt.MapFrom(s => s.Stage != null ? s.Stage.Name : null))
            .ForMember(d => d.StageColor, opt => opt.MapFrom(s => s.Stage != null ? s.Stage.Color : null))
            .ForMember(d => d.CreatedByUserName, opt => opt.MapFrom(s => s.CreatedByUser != null ? $"{s.CreatedByUser.FirstName} {s.CreatedByUser.LastName}" : null))
            .ForMember(d => d.AssignedToUserName, opt => opt.MapFrom(s => s.AssignedToUser != null ? $"{s.AssignedToUser.FirstName} {s.AssignedToUser.LastName}" : null))
            .ForMember(d => d.TasksCount, opt => opt.MapFrom(s => s.Tasks.Count));

        CreateMap<CreateDealDto, Deal>();
        CreateMap<UpdateDealDto, Deal>();

        // Task mappings
        CreateMap<TaskItem, TaskDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer != null ? s.Customer.Name : null))
            .ForMember(d => d.DealTitle, opt => opt.MapFrom(s => s.Deal != null ? s.Deal.Title : null))
            .ForMember(d => d.CreatedByUserName, opt => opt.MapFrom(s => s.CreatedByUser != null ? $"{s.CreatedByUser.FirstName} {s.CreatedByUser.LastName}" : null))
            .ForMember(d => d.AssignedToUserName, opt => opt.MapFrom(s => s.AssignedToUser != null ? $"{s.AssignedToUser.FirstName} {s.AssignedToUser.LastName}" : null))
            .ForMember(d => d.PriorityName, opt => opt.MapFrom(s => TaskPriorityNames.GetName(s.Priority)))
            .ForMember(d => d.StatusName, opt => opt.MapFrom(s => TaskStatusNames.GetName(s.Status)));

        CreateMap<CreateTaskDto, TaskItem>();
        CreateMap<UpdateTaskDto, TaskItem>();

        // ActivityLog mappings
        CreateMap<ActivityLog, ActivityLogDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : null));
    }
}
