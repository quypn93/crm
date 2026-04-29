START TRANSACTION;

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
COMMIT;

