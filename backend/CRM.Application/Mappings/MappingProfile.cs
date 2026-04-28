using AutoMapper;
using CRM.Application.DTOs.Auth;
using CRM.Application.DTOs.Customer;
using CRM.Application.DTOs.Deal;
using CRM.Application.DTOs.Design;
using CRM.Application.DTOs.Location;
using CRM.Application.DTOs.Order;
using CRM.Application.DTOs.Production;
using CRM.Application.DTOs.Task;
using CRM.Application.DTOs.Report;
using CRM.Application.DTOs.User;
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

        CreateMap<User, UserListItemDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<Role, RoleDto>();

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

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer != null ? s.Customer.Name : s.CustomerName))
            .ForMember(d => d.DealTitle, opt => opt.MapFrom(s => s.Deal != null ? s.Deal.Title : null))
            .ForMember(d => d.CreatedByUserName, opt => opt.MapFrom(s => s.CreatedByUser != null ? $"{s.CreatedByUser.FirstName} {s.CreatedByUser.LastName}" : null))
            .ForMember(d => d.AssignedToUserName, opt => opt.MapFrom(s => s.AssignedToUser != null ? $"{s.AssignedToUser.FirstName} {s.AssignedToUser.LastName}" : null))
            .ForMember(d => d.ItemsCount, opt => opt.MapFrom(s => s.Items.Count))
            .ForMember(d => d.DesignerUserName, opt => opt.MapFrom(s => s.DesignerUser != null ? $"{s.DesignerUser.FirstName} {s.DesignerUser.LastName}" : null))
            .ForMember(d => d.ProductionDaysOptionName, opt => opt.MapFrom(s => s.ProductionDaysOption != null ? s.ProductionDaysOption.Name : null))
            .ForMember(d => d.DesignName, opt => opt.MapFrom(s => s.Design != null ? s.Design.DesignName : null))
            .ForMember(d => d.DesignCompletedImageUrl, opt => opt.MapFrom(s => s.Design != null ? s.Design.CompletedImageUrl : null));

        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<CreateOrderItemDto, OrderItem>();

        // Location mappings
        CreateMap<Province, ProvinceDto>();
        CreateMap<Ward, WardDto>();

        // New admin lookups
        CreateMap<Collection, CRM.Application.DTOs.Lookup.CollectionDto>();
        CreateMap<CRM.Application.DTOs.Lookup.CreateCollectionDto, Collection>();
        CreateMap<Material, CRM.Application.DTOs.Lookup.LookupItemDto>();
        CreateMap<ProductForm, CRM.Application.DTOs.Lookup.LookupItemDto>();
        CreateMap<ProductSpecification, CRM.Application.DTOs.Lookup.LookupItemDto>();
        CreateMap<ProductionDaysOption, CRM.Application.DTOs.Lookup.ProductionDaysOptionDto>();
        CreateMap<DepositTransaction, CRM.Application.DTOs.Lookup.DepositTransactionDto>();

        // ColorFabric mappings
        CreateMap<ColorFabric, ColorFabricDto>();
        CreateMap<CreateColorFabricDto, ColorFabric>();
        CreateMap<UpdateColorFabricDto, ColorFabric>();

        // ShirtComponent mappings
        CreateMap<ShirtComponent, ShirtComponentDto>()
            .ForMember(d => d.ColorFabricName, opt => opt.MapFrom(s => s.ColorFabric != null ? s.ColorFabric.Name : null));
        CreateMap<CreateShirtComponentDto, ShirtComponent>();
        CreateMap<UpdateShirtComponentDto, ShirtComponent>();

        // Design mappings
        CreateMap<Design, DesignDto>()
            .ForMember(d => d.ColorFabricName, opt => opt.MapFrom(s => s.ColorFabric != null ? s.ColorFabric.Name : null))
            .ForMember(d => d.AccentColorFabricName, opt => opt.MapFrom(s => s.AccentColorFabric != null ? s.AccentColorFabric.Name : null))
            .ForMember(d => d.ShirtFormName, opt => opt.MapFrom(s => s.ShirtForm != null ? s.ShirtForm.Name : null))
            .ForMember(d => d.AssignedToUserName, opt => opt.MapFrom(s => s.AssignedToUser != null ? $"{s.AssignedToUser.FirstName} {s.AssignedToUser.LastName}" : null))
            .ForMember(d => d.OrderNumber, opt => opt.MapFrom(s => s.Order != null ? s.Order.OrderNumber : null))
            .ForMember(d => d.CreatedByUserName, opt => opt.MapFrom(s => s.CreatedByUser != null ? $"{s.CreatedByUser.FirstName} {s.CreatedByUser.LastName}" : null));

        // ProductionStage mappings
        CreateMap<ProductionStage, ProductionStageDto>();
        CreateMap<CreateProductionStageDto, ProductionStage>();
        CreateMap<UpdateProductionStageDto, ProductionStage>();

        CreateMap<OrderProductionStep, OrderProductionStepDto>()
            .ForMember(d => d.StageOrder, opt => opt.MapFrom(s => s.ProductionStage != null ? s.ProductionStage.StageOrder : 0))
            .ForMember(d => d.StageName, opt => opt.MapFrom(s => s.ProductionStage != null ? s.ProductionStage.StageName : string.Empty))
            .ForMember(d => d.ResponsibleRole, opt => opt.MapFrom(s => s.ProductionStage != null ? s.ProductionStage.ResponsibleRole : null))
            .ForMember(d => d.CompletedByUserName, opt => opt.MapFrom(s =>
                s.CompletedByUser != null ? $"{s.CompletedByUser.FirstName} {s.CompletedByUser.LastName}" : null));

        CreateMap<Design, DesignDetailDto>()
            .ForMember(d => d.ColorFabricName, opt => opt.MapFrom(s => s.ColorFabric != null ? s.ColorFabric.Name : null))
            .ForMember(d => d.OrderNumber, opt => opt.MapFrom(s => s.Order != null ? s.Order.OrderNumber : null))
            .ForMember(d => d.CreatedByUserName, opt => opt.MapFrom(s => s.CreatedByUser != null ? $"{s.CreatedByUser.FirstName} {s.CreatedByUser.LastName}" : null))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Order != null && s.Order.Customer != null ? s.Order.Customer.Name : null))
            .ForMember(d => d.CustomerPhone, opt => opt.MapFrom(s => s.Order != null && s.Order.Customer != null ? s.Order.Customer.Phone : null))
            .ForMember(d => d.CustomerEmail, opt => opt.MapFrom(s => s.Order != null && s.Order.Customer != null ? s.Order.Customer.Email : null));

        CreateMap<CreateDesignDto, Design>();
        CreateMap<UpdateDesignDto, Design>();
    }
}
