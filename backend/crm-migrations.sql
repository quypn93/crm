CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Collections" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Collections" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "ColorFabrics" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Description" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ColorFabrics" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "DealStages" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Order" integer NOT NULL,
        "Color" character varying(20),
        "Probability" integer NOT NULL,
        "IsDefault" boolean NOT NULL,
        "IsWonStage" boolean NOT NULL,
        "IsLostStage" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_DealStages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Materials" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Materials" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "ProductForms" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ProductForms" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "ProductionDaysOptions" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Days" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ProductionDaysOptions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "ProductionStages" (
        "Id" uuid NOT NULL,
        "StageOrder" integer NOT NULL,
        "StageName" character varying(200) NOT NULL,
        "Description" character varying(500),
        "ResponsibleRole" character varying(50),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ProductionStages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "ProductSpecifications" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(1000),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ProductSpecifications" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "Description" character varying(255),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Email" character varying(255) NOT NULL,
        "PasswordHash" character varying(500) NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "PhoneNumber" character varying(20),
        "AvatarUrl" character varying(500),
        "IsActive" boolean NOT NULL,
        "RefreshToken" character varying(500),
        "RefreshTokenExpiryTime" timestamp with time zone,
        "LastLoginAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "CollectionColors" (
        "CollectionId" uuid NOT NULL,
        "ColorFabricId" uuid NOT NULL,
        CONSTRAINT "PK_CollectionColors" PRIMARY KEY ("CollectionId", "ColorFabricId"),
        CONSTRAINT "FK_CollectionColors_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_CollectionColors_ColorFabrics_ColorFabricId" FOREIGN KEY ("ColorFabricId") REFERENCES "ColorFabrics" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "ShirtComponents" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "ImageUrl" character varying(500),
        "WomenImageUrl" character varying(500),
        "Type" integer NOT NULL,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "ColorFabricId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ShirtComponents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ShirtComponents_ColorFabrics_ColorFabricId" FOREIGN KEY ("ColorFabricId") REFERENCES "ColorFabrics" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "CollectionMaterials" (
        "CollectionId" uuid NOT NULL,
        "MaterialId" uuid NOT NULL,
        CONSTRAINT "PK_CollectionMaterials" PRIMARY KEY ("CollectionId", "MaterialId"),
        CONSTRAINT "FK_CollectionMaterials_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_CollectionMaterials_Materials_MaterialId" FOREIGN KEY ("MaterialId") REFERENCES "Materials" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "CollectionForms" (
        "CollectionId" uuid NOT NULL,
        "ProductFormId" uuid NOT NULL,
        CONSTRAINT "PK_CollectionForms" PRIMARY KEY ("CollectionId", "ProductFormId"),
        CONSTRAINT "FK_CollectionForms_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_CollectionForms_ProductForms_ProductFormId" FOREIGN KEY ("ProductFormId") REFERENCES "ProductForms" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "CollectionSpecifications" (
        "CollectionId" uuid NOT NULL,
        "ProductSpecificationId" uuid NOT NULL,
        CONSTRAINT "PK_CollectionSpecifications" PRIMARY KEY ("CollectionId", "ProductSpecificationId"),
        CONSTRAINT "FK_CollectionSpecifications_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_CollectionSpecifications_ProductSpecifications_ProductSpeci~" FOREIGN KEY ("ProductSpecificationId") REFERENCES "ProductSpecifications" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "ActivityLogs" (
        "Id" uuid NOT NULL,
        "EntityType" character varying(50) NOT NULL,
        "EntityId" uuid NOT NULL,
        "Action" character varying(50) NOT NULL,
        "OldValue" text,
        "NewValue" text,
        "Description" character varying(500),
        "UserId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ActivityLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ActivityLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Customers" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Email" character varying(255),
        "Phone" character varying(50),
        "Address" character varying(500),
        "City" character varying(100),
        "Country" character varying(100),
        "PostalCode" character varying(20),
        "CompanyName" character varying(255),
        "Industry" character varying(100),
        "Website" character varying(255),
        "Notes" text,
        "IsActive" boolean NOT NULL,
        "CreatedByUserId" uuid NOT NULL,
        "AssignedToUserId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Customers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Customers_Users_AssignedToUserId" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Customers_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "UserRoles" (
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "AssignedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_UserRoles" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Deals" (
        "Id" uuid NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Value" numeric(18,2) NOT NULL,
        "Currency" character varying(10) NOT NULL DEFAULT 'VND',
        "ExpectedCloseDate" timestamp with time zone,
        "ActualCloseDate" timestamp with time zone,
        "Probability" integer NOT NULL,
        "Notes" text,
        "LostReason" character varying(500),
        "CustomerId" uuid NOT NULL,
        "StageId" uuid NOT NULL,
        "CreatedByUserId" uuid NOT NULL,
        "AssignedToUserId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Deals" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Deals_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Deals_DealStages_StageId" FOREIGN KEY ("StageId") REFERENCES "DealStages" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Deals_Users_AssignedToUserId" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Deals_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Orders" (
        "Id" uuid NOT NULL,
        "OrderNumber" character varying(50) NOT NULL,
        "CustomerId" uuid,
        "CustomerName" text,
        "DealId" uuid,
        "Status" integer NOT NULL,
        "SubTotal" numeric(18,2) NOT NULL,
        "DiscountPercent" numeric(5,2) NOT NULL,
        "DiscountAmount" numeric(18,2) NOT NULL,
        "TaxPercent" numeric(5,2) NOT NULL,
        "TaxAmount" numeric(18,2) NOT NULL,
        "TotalAmount" numeric(18,2) NOT NULL,
        "Currency" character varying(10) NOT NULL DEFAULT 'VND',
        "OrderDate" timestamp with time zone NOT NULL,
        "ExpectedDeliveryDate" timestamp with time zone,
        "CompletionDate" timestamp with time zone,
        "ReturnDate" timestamp with time zone,
        "ActualDeliveryDate" timestamp with time zone,
        "ShippingAddress" character varying(500),
        "ShippingCity" character varying(100),
        "ShippingPhone" character varying(50),
        "ShippingNotes" text,
        "PaymentStatus" integer NOT NULL,
        "PaymentMethod" character varying(100),
        "PaidAmount" numeric(18,2) NOT NULL,
        "PaymentDate" timestamp with time zone,
        "Notes" text,
        "InternalNotes" text,
        "StyleNotes" text,
        "ProductionDaysOptionId" uuid,
        "ProductionDays" integer,
        "DepositCode" character varying(100),
        "DesignImageUrl" character varying(500),
        "QrCodeToken" text,
        "QrCodeImageBase64" text,
        "CreatedByUserId" uuid NOT NULL,
        "AssignedToUserId" uuid,
        "DesignerUserId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Orders_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Orders_Deals_DealId" FOREIGN KEY ("DealId") REFERENCES "Deals" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Orders_ProductionDaysOptions_ProductionDaysOptionId" FOREIGN KEY ("ProductionDaysOptionId") REFERENCES "ProductionDaysOptions" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Orders_Users_AssignedToUserId" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Orders_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Orders_Users_DesignerUserId" FOREIGN KEY ("DesignerUserId") REFERENCES "Users" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Tasks" (
        "Id" uuid NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Description" text,
        "DueDate" timestamp with time zone,
        "ReminderDate" timestamp with time zone,
        "Priority" integer NOT NULL DEFAULT 2,
        "Status" integer NOT NULL DEFAULT 0,
        "CompletedAt" timestamp with time zone,
        "CustomerId" uuid,
        "DealId" uuid,
        "AssignedToUserId" uuid,
        "CreatedByUserId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Tasks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Tasks_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Tasks_Deals_DealId" FOREIGN KEY ("DealId") REFERENCES "Deals" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Tasks_Users_AssignedToUserId" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Tasks_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "DepositTransactions" (
        "Id" uuid NOT NULL,
        "Code" character varying(100) NOT NULL,
        "Amount" numeric(18,2) NOT NULL,
        "BankName" character varying(100) NOT NULL,
        "AccountNumber" character varying(50),
        "Description" character varying(500),
        "TransactionDate" timestamp with time zone NOT NULL,
        "Source" character varying(20) NOT NULL,
        "CassoId" character varying(100),
        "MatchedOrderId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_DepositTransactions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DepositTransactions_Orders_MatchedOrderId" FOREIGN KEY ("MatchedOrderId") REFERENCES "Orders" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "Designs" (
        "Id" uuid NOT NULL,
        "DesignName" character varying(200) NOT NULL,
        "DesignData" text,
        "SelectedComponents" text,
        "Designer" character varying(200),
        "CustomerFullName" character varying(200),
        "Total" integer,
        "SizeMan" character varying(500),
        "SizeWomen" character varying(500),
        "SizeKid" character varying(500),
        "Oversized" character varying(500),
        "FrontImageUrl" text,
        "BackImageUrl" text,
        "SizeQuantities" text,
        "PersonNamesBySize" text,
        "MaterialText" text,
        "ColorText" text,
        "StyleText" text,
        "ReturnDate" timestamp with time zone,
        "GiftItems" text,
        "FinishedDate" timestamp with time zone,
        "NoteConfection" text,
        "NoteOldCodeOrder" character varying(200),
        "NoteAttachTagLabel" character varying(500),
        "NoteOther" text,
        "SaleStaff" character varying(200),
        "ColorFabricId" uuid,
        "OrderId" uuid,
        "CreatedByUserId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Designs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Designs_ColorFabrics_ColorFabricId" FOREIGN KEY ("ColorFabricId") REFERENCES "ColorFabrics" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Designs_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Designs_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "OrderItems" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "CollectionId" uuid,
        "CollectionName" character varying(255),
        "ProductCode" character varying(50),
        "Description" character varying(1000),
        "Size" character varying(50),
        "MainColorId" uuid,
        "AccentColorId" uuid,
        "MaterialId" uuid,
        "FormId" uuid,
        "SpecificationId" uuid,
        "MainColorName" character varying(100),
        "AccentColorName" character varying(100),
        "MaterialName" character varying(100),
        "FormName" character varying(100),
        "SpecificationName" character varying(100),
        "Quantity" integer NOT NULL,
        "Unit" character varying(20) NOT NULL DEFAULT 'cái',
        "UnitPrice" numeric(18,2) NOT NULL,
        "DiscountPercent" numeric(5,2) NOT NULL,
        "DiscountAmount" numeric(18,2) NOT NULL,
        "LineTotal" numeric(18,2) NOT NULL,
        "Notes" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrderItems_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE TABLE "OrderProductionSteps" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "ProductionStageId" uuid NOT NULL,
        "IsCompleted" boolean NOT NULL,
        "CompletedByUserId" uuid,
        "CompletedAt" timestamp with time zone,
        "Notes" character varying(1000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_OrderProductionSteps" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrderProductionSteps_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OrderProductionSteps_ProductionStages_ProductionStageId" FOREIGN KEY ("ProductionStageId") REFERENCES "ProductionStages" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OrderProductionSteps_Users_CompletedByUserId" FOREIGN KEY ("CompletedByUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    INSERT INTO "DealStages" ("Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt")
    VALUES ('11111111-aaaa-bbbb-cccc-dddddddddddd', '#14B8A6', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, FALSE, FALSE, 'Đang sản xuất', 5, 90, NULL);
    INSERT INTO "DealStages" ("Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt")
    VALUES ('22222222-aaaa-bbbb-cccc-dddddddddddd', '#0EA5E9', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, FALSE, FALSE, 'Giao hàng', 6, 95, NULL);
    INSERT INTO "DealStages" ("Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt")
    VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '#6366F1', TIMESTAMPTZ '2024-01-01T00:00:00Z', TRUE, FALSE, FALSE, 'Tiềm năng', 1, 10, NULL);
    INSERT INTO "DealStages" ("Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt")
    VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '#8B5CF6', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, FALSE, FALSE, 'Báo giá', 2, 25, NULL);
    INSERT INTO "DealStages" ("Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt")
    VALUES ('cccccccc-cccc-cccc-cccc-cccccccccccc', '#EC4899', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, FALSE, FALSE, 'Duyệt mẫu', 3, 50, NULL);
    INSERT INTO "DealStages" ("Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt")
    VALUES ('dddddddd-dddd-dddd-dddd-dddddddddddd', '#F59E0B', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, FALSE, FALSE, 'Xác nhận đơn', 4, 75, NULL);
    INSERT INTO "DealStages" ("Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt")
    VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '#10B981', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, FALSE, TRUE, 'Hoàn thành', 7, 100, NULL);
    INSERT INTO "DealStages" ("Id", "Color", "CreatedAt", "IsDefault", "IsLostStage", "IsWonStage", "Name", "Order", "Probability", "UpdatedAt")
    VALUES ('ffffffff-ffff-ffff-ffff-ffffffffffff', '#EF4444', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, TRUE, FALSE, 'Đã hủy', 8, 0, NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('11111111-1111-1111-1111-111111111111', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Quản trị viên hệ thống', 'Admin', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('12121212-1212-1212-1212-121212121212', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên đóng gói', 'PackagingStaff', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('22222222-2222-2222-2222-222222222222', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Quản lý kinh doanh', 'SalesManager', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('33333333-3333-3333-3333-333333333333', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên kinh doanh', 'SalesRep', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('44444444-4444-4444-4444-444444444444', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Quản lý sản xuất', 'ProductionManager', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('55555555-5555-5555-5555-555555555555', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên sản xuất (đa năng)', 'ProductionStaff', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('66666666-6666-6666-6666-666666666666', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Quản lý kiểm soát chất lượng', 'QualityManager', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('77777777-7777-7777-7777-777777777777', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên kiểm soát chất lượng', 'QualityControl', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('88888888-8888-8888-8888-888888888888', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Quản lý giao hàng', 'DeliveryManager', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên giao hàng', 'DeliveryStaff', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Quản lý thiết kế', 'DesignManager', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên thiết kế', 'Designer', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('cccccccc-cccc-cccc-cccc-cccccccccccc', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên cắt vải', 'CuttingStaff', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('dddddddd-dddd-dddd-dddd-dddddddddddd', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên may', 'SewingStaff', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên in / thêu logo', 'PrintingStaff', NULL);
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('ffffffff-ffff-ffff-ffff-ffffffffffff', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên hoàn thiện', 'FinishingStaff', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ActivityLogs_CreatedAt" ON "ActivityLogs" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ActivityLogs_EntityType_EntityId" ON "ActivityLogs" ("EntityType", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ActivityLogs_UserId" ON "ActivityLogs" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_CollectionColors_ColorFabricId" ON "CollectionColors" ("ColorFabricId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_CollectionForms_ProductFormId" ON "CollectionForms" ("ProductFormId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_CollectionMaterials_MaterialId" ON "CollectionMaterials" ("MaterialId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Collections_Name" ON "Collections" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_CollectionSpecifications_ProductSpecificationId" ON "CollectionSpecifications" ("ProductSpecificationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ColorFabrics_Name" ON "ColorFabrics" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Customers_AssignedToUserId" ON "Customers" ("AssignedToUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Customers_CompanyName" ON "Customers" ("CompanyName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Customers_CreatedByUserId" ON "Customers" ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Customers_Email" ON "Customers" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Customers_Name" ON "Customers" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Deals_AssignedToUserId" ON "Deals" ("AssignedToUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Deals_CreatedByUserId" ON "Deals" ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Deals_CustomerId" ON "Deals" ("CustomerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Deals_ExpectedCloseDate" ON "Deals" ("ExpectedCloseDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Deals_StageId" ON "Deals" ("StageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_DepositTransactions_CassoId" ON "DepositTransactions" ("CassoId") WHERE "CassoId" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_DepositTransactions_Code" ON "DepositTransactions" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_DepositTransactions_MatchedOrderId" ON "DepositTransactions" ("MatchedOrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Designs_ColorFabricId" ON "Designs" ("ColorFabricId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Designs_CreatedAt" ON "Designs" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Designs_CreatedByUserId" ON "Designs" ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Designs_OrderId" ON "Designs" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Materials_Name" ON "Materials" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_OrderItems_CollectionId" ON "OrderItems" ("CollectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_OrderProductionSteps_CompletedByUserId" ON "OrderProductionSteps" ("CompletedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_OrderProductionSteps_IsCompleted" ON "OrderProductionSteps" ("IsCompleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_OrderProductionSteps_OrderId" ON "OrderProductionSteps" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_OrderProductionSteps_OrderId_ProductionStageId" ON "OrderProductionSteps" ("OrderId", "ProductionStageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_OrderProductionSteps_ProductionStageId" ON "OrderProductionSteps" ("ProductionStageId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_AssignedToUserId" ON "Orders" ("AssignedToUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_CreatedByUserId" ON "Orders" ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_DealId" ON "Orders" ("DealId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_DesignerUserId" ON "Orders" ("DesignerUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_OrderDate" ON "Orders" ("OrderDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Orders_OrderNumber" ON "Orders" ("OrderNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_PaymentStatus" ON "Orders" ("PaymentStatus");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_ProductionDaysOptionId" ON "Orders" ("ProductionDaysOptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Orders_Status" ON "Orders" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ProductForms_Name" ON "ProductForms" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ProductionStages_IsActive" ON "ProductionStages" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ProductionStages_StageOrder" ON "ProductionStages" ("StageOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ProductSpecifications_Name" ON "ProductSpecifications" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ShirtComponents_ColorFabricId" ON "ShirtComponents" ("ColorFabricId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ShirtComponents_IsDeleted" ON "ShirtComponents" ("IsDeleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_ShirtComponents_Type" ON "ShirtComponents" ("Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Tasks_AssignedToUserId" ON "Tasks" ("AssignedToUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Tasks_CreatedByUserId" ON "Tasks" ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Tasks_CustomerId" ON "Tasks" ("CustomerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Tasks_DealId" ON "Tasks" ("DealId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Tasks_DueDate" ON "Tasks" ("DueDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_Tasks_Status" ON "Tasks" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260420114923_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260420114923_InitialCreate', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    ALTER TABLE "Orders" ADD "ShippingContactName" character varying(150);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    ALTER TABLE "Orders" ADD "ShippingProvinceCode" character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    ALTER TABLE "Orders" ADD "ShippingProvinceName" character varying(150);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    ALTER TABLE "Orders" ADD "ShippingWardCode" character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    ALTER TABLE "Orders" ADD "ShippingWardName" character varying(250);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    CREATE TABLE "Provinces" (
        "Code" character varying(10) NOT NULL,
        "Name" character varying(100) NOT NULL,
        "FullName" character varying(150) NOT NULL,
        "Type" character varying(50) NOT NULL,
        "SortOrder" integer NOT NULL,
        CONSTRAINT "PK_Provinces" PRIMARY KEY ("Code")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    CREATE TABLE "Wards" (
        "Code" character varying(10) NOT NULL,
        "Name" character varying(150) NOT NULL,
        "FullName" character varying(250) NOT NULL,
        "Type" character varying(50) NOT NULL,
        "ProvinceCode" character varying(10) NOT NULL,
        CONSTRAINT "PK_Wards" PRIMARY KEY ("Code"),
        CONSTRAINT "FK_Wards_Provinces_ProvinceCode" FOREIGN KEY ("ProvinceCode") REFERENCES "Provinces" ("Code") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    CREATE INDEX "IX_Provinces_Name" ON "Provinces" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    CREATE INDEX "IX_Wards_Name" ON "Wards" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    CREATE INDEX "IX_Wards_ProvinceCode" ON "Wards" ("ProvinceCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423145406_AddLocationsAndOrderShipping') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260423145406_AddLocationsAndOrderShipping', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423171404_AddOrderDeliveryMethod') THEN
    ALTER TABLE "Orders" ADD "DeliveryMethod" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260423171404_AddOrderDeliveryMethod') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260423171404_AddOrderDeliveryMethod', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    ALTER TABLE "Orders" ADD "GhtkFee" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    ALTER TABLE "Orders" ADD "GhtkInsuranceFee" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    ALTER TABLE "Orders" ADD "GhtkLabel" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    ALTER TABLE "Orders" ADD "GhtkLastError" character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    ALTER TABLE "Orders" ADD "GhtkStatus" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    ALTER TABLE "Orders" ADD "GhtkStatusCode" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    ALTER TABLE "Orders" ADD "GhtkSyncedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    ALTER TABLE "Orders" ADD "GhtkTrackingUrl" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    CREATE INDEX "IX_Orders_GhtkLabel" ON "Orders" ("GhtkLabel");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424030013_AddGhtkTrackingToOrder') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260424030013_AddGhtkTrackingToOrder', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Orders" ADD "DesignId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "AccentColorFabricId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "AssignedToUserId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "AssignmentNotes" character varying(1000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "BackLogoUrl" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "ChestLogoUrl" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "CompletedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "CompletedImageUrl" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "ShirtFormId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD "Status" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    CREATE INDEX "IX_Orders_DesignId" ON "Orders" ("DesignId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    CREATE INDEX "IX_Designs_AccentColorFabricId" ON "Designs" ("AccentColorFabricId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    CREATE INDEX "IX_Designs_AssignedToUserId" ON "Designs" ("AssignedToUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    CREATE INDEX "IX_Designs_ShirtFormId" ON "Designs" ("ShirtFormId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    CREATE INDEX "IX_Designs_Status" ON "Designs" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD CONSTRAINT "FK_Designs_ColorFabrics_AccentColorFabricId" FOREIGN KEY ("AccentColorFabricId") REFERENCES "ColorFabrics" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD CONSTRAINT "FK_Designs_ProductForms_ShirtFormId" FOREIGN KEY ("ShirtFormId") REFERENCES "ProductForms" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Designs" ADD CONSTRAINT "FK_Designs_Users_AssignedToUserId" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    ALTER TABLE "Orders" ADD CONSTRAINT "FK_Orders_Designs_DesignId" FOREIGN KEY ("DesignId") REFERENCES "Designs" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260424053629_AddDesignAssignmentFlow') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260424053629_AddDesignAssignmentFlow', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260426000000_AddContentStaffRole') THEN
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('13131313-1313-1313-1313-131313131313', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Nhân viên content (giao việc cho design)', 'ContentStaff', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260426000000_AddContentStaffRole') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260426000000_AddContentStaffRole', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260426010000_AddContentManagerRole') THEN
    INSERT INTO "Roles" ("Id", "CreatedAt", "Description", "Name", "UpdatedAt")
    VALUES ('14141414-1414-1414-1414-141414141414', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Trưởng phòng content', 'ContentManager', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260426010000_AddContentManagerRole') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260426010000_AddContentManagerRole', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260429000000_ReseedFullVietnamWards') THEN
    DELETE FROM "Wards";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260429000000_ReseedFullVietnamWards') THEN
    DELETE FROM "Provinces";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260429000000_ReseedFullVietnamWards') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260429000000_ReseedFullVietnamWards', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502023230_AddNotifications') THEN
    CREATE TABLE "NotificationRolePreferences" (
        "Id" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "Type" integer NOT NULL,
        "InApp" boolean NOT NULL,
        "Email" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_NotificationRolePreferences" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_NotificationRolePreferences_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502023230_AddNotifications') THEN
    CREATE TABLE "Notifications" (
        "Id" uuid NOT NULL,
        "RecipientUserId" uuid NOT NULL,
        "Type" integer NOT NULL,
        "Severity" integer NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Message" character varying(1000) NOT NULL,
        "Link" character varying(500),
        "EntityType" character varying(64),
        "EntityId" uuid,
        "IsRead" boolean NOT NULL,
        "ReadAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Notifications_Users_RecipientUserId" FOREIGN KEY ("RecipientUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502023230_AddNotifications') THEN
    CREATE TABLE "TaskNotificationLogs" (
        "Id" uuid NOT NULL,
        "TaskId" uuid NOT NULL,
        "Type" integer NOT NULL,
        "SentAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_TaskNotificationLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TaskNotificationLogs_Tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502023230_AddNotifications') THEN
    CREATE UNIQUE INDEX "IX_NotificationRolePreferences_RoleId_Type" ON "NotificationRolePreferences" ("RoleId", "Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502023230_AddNotifications') THEN
    CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502023230_AddNotifications') THEN
    CREATE INDEX "IX_Notifications_Recipient_Unread_Created" ON "Notifications" ("RecipientUserId", "IsRead", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502023230_AddNotifications') THEN
    CREATE UNIQUE INDEX "IX_TaskNotificationLogs_TaskId_Type" ON "TaskNotificationLogs" ("TaskId", "Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260502023230_AddNotifications') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260502023230_AddNotifications', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE TABLE "Conversations" (
        "Id" uuid NOT NULL,
        "Type" integer NOT NULL,
        "Name" character varying(200),
        "CreatedByUserId" uuid NOT NULL,
        "LastMessageId" uuid,
        "LastMessageAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Conversations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Conversations_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE TABLE "ConversationParticipants" (
        "Id" uuid NOT NULL,
        "ConversationId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "IsAdmin" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "JoinedAt" timestamp with time zone NOT NULL,
        "LeftAt" timestamp with time zone,
        "LastReadAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ConversationParticipants" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ConversationParticipants_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ConversationParticipants_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE TABLE "ChatMessages" (
        "Id" uuid NOT NULL,
        "ConversationId" uuid NOT NULL,
        "SenderUserId" uuid NOT NULL,
        "Content" character varying(4000) NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ChatMessages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ChatMessages_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ChatMessages_Users_SenderUserId" FOREIGN KEY ("SenderUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE INDEX "IX_Conversations_CreatedByUserId" ON "Conversations" ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE INDEX "IX_Conversations_LastMessageAt" ON "Conversations" ("LastMessageAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE INDEX "IX_ConversationParticipants_UserId" ON "ConversationParticipants" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE UNIQUE INDEX "IX_ConversationParticipants_Conversation_User_Active" ON "ConversationParticipants" ("ConversationId", "UserId") WHERE "IsActive" = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE INDEX "IX_ChatMessages_Conversation_Created" ON "ChatMessages" ("ConversationId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    CREATE INDEX "IX_ChatMessages_SenderUserId" ON "ChatMessages" ("SenderUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260507090000_AddChat') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260507090000_AddChat', '9.0.1');
    END IF;
END $EF$;
COMMIT;

