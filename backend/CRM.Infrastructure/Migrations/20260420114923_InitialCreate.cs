using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColorFabrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorFabrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DealStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Probability = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsWonStage = table.Column<bool>(type: "boolean", nullable: false),
                    IsLostStage = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionDaysOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionDaysOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StageOrder = table.Column<int>(type: "integer", nullable: false),
                    StageName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponsibleRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductSpecifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollectionColors",
                columns: table => new
                {
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ColorFabricId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionColors", x => new { x.CollectionId, x.ColorFabricId });
                    table.ForeignKey(
                        name: "FK_CollectionColors_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionColors_ColorFabrics_ColorFabricId",
                        column: x => x.ColorFabricId,
                        principalTable: "ColorFabrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShirtComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WomenImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ColorFabricId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShirtComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShirtComponents_ColorFabrics_ColorFabricId",
                        column: x => x.ColorFabricId,
                        principalTable: "ColorFabrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CollectionMaterials",
                columns: table => new
                {
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionMaterials", x => new { x.CollectionId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_CollectionMaterials_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionForms",
                columns: table => new
                {
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductFormId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionForms", x => new { x.CollectionId, x.ProductFormId });
                    table.ForeignKey(
                        name: "FK_CollectionForms_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionForms_ProductForms_ProductFormId",
                        column: x => x.ProductFormId,
                        principalTable: "ProductForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollectionSpecifications",
                columns: table => new
                {
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductSpecificationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSpecifications", x => new { x.CollectionId, x.ProductSpecificationId });
                    table.ForeignKey(
                        name: "FK_CollectionSpecifications_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionSpecifications_ProductSpecifications_ProductSpeci~",
                        column: x => x.ProductSpecificationId,
                        principalTable: "ProductSpecifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Customers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    ExpectedCloseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualCloseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Probability = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    LostReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deals_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_DealStages_StageId",
                        column: x => x.StageId,
                        principalTable: "DealStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Deals_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    DealId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShippingAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShippingCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShippingPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShippingNotes = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    StyleNotes = table.Column<string>(type: "text", nullable: true),
                    ProductionDaysOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductionDays = table.Column<int>(type: "integer", nullable: true),
                    DepositCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DesignImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QrCodeToken = table.Column<string>(type: "text", nullable: true),
                    QrCodeImageBase64 = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DesignerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Deals_DealId",
                        column: x => x.DealId,
                        principalTable: "Deals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_ProductionDaysOptions_ProductionDaysOptionId",
                        column: x => x.ProductionDaysOptionId,
                        principalTable: "ProductionDaysOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_DesignerUserId",
                        column: x => x.DesignerUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReminderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DealId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tasks_Deals_DealId",
                        column: x => x.DealId,
                        principalTable: "Deals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepositTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CassoId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MatchedOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepositTransactions_Orders_MatchedOrderId",
                        column: x => x.MatchedOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Designs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DesignName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DesignData = table.Column<string>(type: "text", nullable: true),
                    SelectedComponents = table.Column<string>(type: "text", nullable: true),
                    Designer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CustomerFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Total = table.Column<int>(type: "integer", nullable: true),
                    SizeMan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SizeWomen = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SizeKid = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Oversized = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FrontImageUrl = table.Column<string>(type: "text", nullable: true),
                    BackImageUrl = table.Column<string>(type: "text", nullable: true),
                    SizeQuantities = table.Column<string>(type: "text", nullable: true),
                    PersonNamesBySize = table.Column<string>(type: "text", nullable: true),
                    MaterialText = table.Column<string>(type: "text", nullable: true),
                    ColorText = table.Column<string>(type: "text", nullable: true),
                    StyleText = table.Column<string>(type: "text", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GiftItems = table.Column<string>(type: "text", nullable: true),
                    FinishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NoteConfection = table.Column<string>(type: "text", nullable: true),
                    NoteOldCodeOrder = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NoteAttachTagLabel = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NoteOther = table.Column<string>(type: "text", nullable: true),
                    SaleStaff = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ColorFabricId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Designs_ColorFabrics_ColorFabricId",
                        column: x => x.ColorFabricId,
                        principalTable: "ColorFabrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Designs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Designs_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CollectionName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ProductCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MainColorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccentColorId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpecificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    MainColorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccentColorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaterialName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FormName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SpecificationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "cái"),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderProductionSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionStageId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderProductionSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderProductionSteps_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderProductionSteps_ProductionStages_ProductionStageId",
                        column: x => x.ProductionStageId,
                        principalTable: "ProductionStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderProductionSteps_Users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "DealStages",
                columns: new[] { "Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-aaaa-bbbb-cccc-dddddddddddd"), "#14B8A6", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, false, "Đang sản xuất", 5, 90, null },
                    { new Guid("22222222-aaaa-bbbb-cccc-dddddddddddd"), "#0EA5E9", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, false, "Giao hàng", 6, 95, null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "#6366F1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, false, "Tiềm năng", 1, 10, null },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "#8B5CF6", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, false, "Báo giá", 2, 25, null },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "#EC4899", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, false, "Duyệt mẫu", 3, 50, null },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "#F59E0B", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, false, "Xác nhận đơn", 4, 75, null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "#10B981", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, true, "Hoàn thành", 7, 100, null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "#EF4444", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, false, "Đã hủy", 8, 0, null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quản trị viên hệ thống", "Admin", null },
                    { new Guid("12121212-1212-1212-1212-121212121212"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên đóng gói", "PackagingStaff", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quản lý kinh doanh", "SalesManager", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên kinh doanh", "SalesRep", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quản lý sản xuất", "ProductionManager", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên sản xuất (đa năng)", "ProductionStaff", null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quản lý kiểm soát chất lượng", "QualityManager", null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên kiểm soát chất lượng", "QualityControl", null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quản lý giao hàng", "DeliveryManager", null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên giao hàng", "DeliveryStaff", null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quản lý thiết kế", "DesignManager", null },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên thiết kế", "Designer", null },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên cắt vải", "CuttingStaff", null },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên may", "SewingStaff", null },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên in / thêu logo", "PrintingStaff", null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên hoàn thiện", "FinishingStaff", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "ActivityLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_EntityType_EntityId",
                table: "ActivityLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_UserId",
                table: "ActivityLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionColors_ColorFabricId",
                table: "CollectionColors",
                column: "ColorFabricId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionForms_ProductFormId",
                table: "CollectionForms",
                column: "ProductFormId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionMaterials_MaterialId",
                table: "CollectionMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_Name",
                table: "Collections",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSpecifications_ProductSpecificationId",
                table: "CollectionSpecifications",
                column: "ProductSpecificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorFabrics_Name",
                table: "ColorFabrics",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AssignedToUserId",
                table: "Customers",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyName",
                table: "Customers",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedByUserId",
                table: "Customers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_AssignedToUserId",
                table: "Deals",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CreatedByUserId",
                table: "Deals",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CustomerId",
                table: "Deals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_ExpectedCloseDate",
                table: "Deals",
                column: "ExpectedCloseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_StageId",
                table: "Deals",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositTransactions_CassoId",
                table: "DepositTransactions",
                column: "CassoId",
                unique: true,
                filter: "\"CassoId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DepositTransactions_Code",
                table: "DepositTransactions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DepositTransactions_MatchedOrderId",
                table: "DepositTransactions",
                column: "MatchedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_ColorFabricId",
                table: "Designs",
                column: "ColorFabricId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_CreatedAt",
                table: "Designs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_CreatedByUserId",
                table: "Designs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_OrderId",
                table: "Designs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Name",
                table: "Materials",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_CollectionId",
                table: "OrderItems",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_CompletedByUserId",
                table: "OrderProductionSteps",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_IsCompleted",
                table: "OrderProductionSteps",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_OrderId",
                table: "OrderProductionSteps",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_OrderId_ProductionStageId",
                table: "OrderProductionSteps",
                columns: new[] { "OrderId", "ProductionStageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderProductionSteps_ProductionStageId",
                table: "OrderProductionSteps",
                column: "ProductionStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AssignedToUserId",
                table: "Orders",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedByUserId",
                table: "Orders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DealId",
                table: "Orders",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DesignerUserId",
                table: "Orders",
                column: "DesignerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentStatus",
                table: "Orders",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductionDaysOptionId",
                table: "Orders",
                column: "ProductionDaysOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductForms_Name",
                table: "ProductForms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_IsActive",
                table: "ProductionStages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_StageOrder",
                table: "ProductionStages",
                column: "StageOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_Name",
                table: "ProductSpecifications",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShirtComponents_ColorFabricId",
                table: "ShirtComponents",
                column: "ColorFabricId");

            migrationBuilder.CreateIndex(
                name: "IX_ShirtComponents_IsDeleted",
                table: "ShirtComponents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ShirtComponents_Type",
                table: "ShirtComponents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToUserId",
                table: "Tasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedByUserId",
                table: "Tasks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CustomerId",
                table: "Tasks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DealId",
                table: "Tasks",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status",
                table: "Tasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "CollectionColors");

            migrationBuilder.DropTable(
                name: "CollectionForms");

            migrationBuilder.DropTable(
                name: "CollectionMaterials");

            migrationBuilder.DropTable(
                name: "CollectionSpecifications");

            migrationBuilder.DropTable(
                name: "DepositTransactions");

            migrationBuilder.DropTable(
                name: "Designs");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "OrderProductionSteps");

            migrationBuilder.DropTable(
                name: "ShirtComponents");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "ProductForms");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "ProductSpecifications");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ProductionStages");

            migrationBuilder.DropTable(
                name: "ColorFabrics");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Deals");

            migrationBuilder.DropTable(
                name: "ProductionDaysOptions");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "DealStages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
