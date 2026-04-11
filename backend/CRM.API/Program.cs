using System.Text;
using CRM.Application.Mappings;
using CRM.Application.Services;
using CRM.Application.Interfaces;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CRM.Core.Entities;
using CRM.API.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM API - Đồng Phục Bốn Mùa",
        Version = "v1",
        Description = "API cho hệ thống quản lý khách hàng và đơn hàng đồng phục",
        Contact = new OpenApiContact
        {
            Name = "Đồng Phục Bốn Mùa",
            Email = "dongphucbonmua@gmail.com"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token theo định dạng: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    // Order Management Policies
    options.AddPolicy(Policies.CanManageOrders, policy =>
        policy.RequireRole(RoleNames.AllRoles));

    options.AddPolicy(Policies.CanUpdatePayment, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    options.AddPolicy(Policies.CanDeleteOrders, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep));

    options.AddPolicy(Policies.CanViewOrderSummary, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    // Customer Management Policies
    options.AddPolicy(Policies.CanDeleteCustomers, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    // Deal Management Policies
    options.AddPolicy(Policies.CanDeleteDeals, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    options.AddPolicy(Policies.CanCloseDeal, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep));

    // Dashboard Policies
    options.AddPolicy(Policies.CanViewFullDashboard, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    options.AddPolicy(Policies.CanViewProductionDashboard, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.ProductionManager));

    options.AddPolicy(Policies.CanViewQCDashboard, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.QualityControl));

    options.AddPolicy(Policies.CanViewDeliveryDashboard, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.DeliveryManager));

    // Reports Policies
    options.AddPolicy(Policies.CanViewReports, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    options.AddPolicy(Policies.CanExportReports, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    // Design Management Policies
    options.AddPolicy(Policies.CanManageDesigns, policy =>
        policy.RequireRole(RoleNames.AllRoles));

    options.AddPolicy(Policies.CanDeleteDesigns, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    options.AddPolicy(Policies.CanManageColorFabrics, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    options.AddPolicy(Policies.CanManageShirtComponents, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IProductionStageService, ProductionStageService>();
builder.Services.AddScoped<IOrderProductionService, OrderProductionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IColorFabricService, ColorFabricService>();
builder.Services.AddScoped<IShirtComponentService, ShirtComponentService>();
builder.Services.AddScoped<IDesignService, DesignService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedAsync(db);
}

app.Run();
