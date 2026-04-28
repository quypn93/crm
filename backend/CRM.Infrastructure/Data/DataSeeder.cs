using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(CrmDbContext context)
    {
        await SeedDealStagesAsync(context);
        await context.SaveChangesAsync();

        await SeedUsersAsync(context);
        await context.SaveChangesAsync();

        await EnsureContentStaffAsync(context);
        await context.SaveChangesAsync();

        await SeedSampleDataAsync(context);
        await context.SaveChangesAsync();

        await SeedColorFabricsAsync(context);
        await context.SaveChangesAsync();

        await SeedShirtComponentsAsync(context);
        await context.SaveChangesAsync();

        await SeedProductionStagesAsync(context);
        await context.SaveChangesAsync();

        await SeedLookupsAsync(context);
        await context.SaveChangesAsync();

        await SeedVietnamLocationsAsync(context);
        await context.SaveChangesAsync();
    }

    // Seed 34 tỉnh + xã/phường từ JSON file. Thay file JSON để bổ sung đủ dữ liệu chính thức.
    private static async Task SeedVietnamLocationsAsync(CrmDbContext context)
    {
        var seedsDir = Path.Combine(AppContext.BaseDirectory, "Data", "Seeds");
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (!await context.Provinces.AnyAsync())
        {
            var provincesPath = Path.Combine(seedsDir, "vietnam-provinces.json");
            if (File.Exists(provincesPath))
            {
                var json = await File.ReadAllTextAsync(provincesPath);
                var items = JsonSerializer.Deserialize<List<Province>>(json, jsonOptions) ?? new();
                context.Provinces.AddRange(items);
            }
        }

        if (!await context.Wards.AnyAsync())
        {
            var wardsPath = Path.Combine(seedsDir, "vietnam-wards.json");
            if (File.Exists(wardsPath))
            {
                // Lưu provinces trước (nếu là lần đầu seed) để FK hợp lệ.
                await context.SaveChangesAsync();

                var json = await File.ReadAllTextAsync(wardsPath);
                var items = JsonSerializer.Deserialize<List<Ward>>(json, jsonOptions) ?? new();

                // Điền FullName nếu JSON không cung cấp, để hiển thị nhanh.
                var provinceMap = await context.Provinces
                    .ToDictionaryAsync(p => p.Code, p => p.FullName);
                foreach (var w in items)
                {
                    if (string.IsNullOrWhiteSpace(w.FullName) && provinceMap.TryGetValue(w.ProvinceCode, out var pFull))
                        w.FullName = $"{w.Name}, {pFull}";
                }

                context.Wards.AddRange(items);
            }
        }
    }

    private static async Task SeedDealStagesAsync(CrmDbContext context)
    {
        // Stages are seeded via migration
    }

    private static User MakeUser(string email, string password, string firstName, string lastName, string phone)
        => new()
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    private static async Task SeedUsersAsync(CrmDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var roles = await context.Roles.ToListAsync();
        Role R(string name) => roles.First(r => r.Name == name);

        // helper: create user + role in one step
        async Task AddUser(User user, string roleName)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = R(roleName).Id });
            await context.SaveChangesAsync();
        }

        async Task AddUsers(User[] users, string roleName)
        {
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
            foreach (var u in users)
                context.UserRoles.Add(new UserRole { UserId = u.Id, RoleId = R(roleName).Id });
            await context.SaveChangesAsync();
        }

        await AddUser(MakeUser("admin@crm.com",            "Admin@123",    "Admin",  "System",    "0901234567"), RoleNames.Admin);
        await AddUser(MakeUser("sales.manager@crm.com",    "Manager@123",  "Nguyen", "Quan Ly",   "0912000001"), RoleNames.SalesManager);
        await AddUsers(new[] {
            MakeUser("sale1@crm.com", "Sale@123", "Sale", "1", "0912001001"),
            MakeUser("sale2@crm.com", "Sale@123", "Sale", "2", "0912001002"),
            MakeUser("sale3@crm.com", "Sale@123", "Sale", "3", "0912001003"),
            MakeUser("sale4@crm.com", "Sale@123", "Sale", "4", "0912001004"),
            MakeUser("sale5@crm.com", "Sale@123", "Sale", "5", "0912001005"),
        }, RoleNames.SalesRep);

        await AddUser(MakeUser("production.manager@crm.com", "Manager@123", "Le",    "San Xuat",   "0923000001"), RoleNames.ProductionManager);
        await AddUsers(new[] {
            MakeUser("worker1@crm.com", "Worker@123", "Worker", "1", "0923001001"),
            MakeUser("worker2@crm.com", "Worker@123", "Worker", "2", "0923001002"),
            MakeUser("worker3@crm.com", "Worker@123", "Worker", "3", "0923001003"),
            MakeUser("worker4@crm.com", "Worker@123", "Worker", "4", "0923001004"),
            MakeUser("worker5@crm.com", "Worker@123", "Worker", "5", "0923001005"),
        }, RoleNames.ProductionStaff);

        // ─── Nhân viên chuyên môn theo khâu (mỗi khâu một role) ────────────
        await AddUsers(new[] {
            MakeUser("cutting1@crm.com", "Cutting@123", "Cutting", "1", "0923101001"),
            MakeUser("cutting2@crm.com", "Cutting@123", "Cutting", "2", "0923101002"),
        }, RoleNames.CuttingStaff);

        await AddUsers(new[] {
            MakeUser("sewing1@crm.com", "Sewing@123", "Sewing", "1", "0923102001"),
            MakeUser("sewing2@crm.com", "Sewing@123", "Sewing", "2", "0923102002"),
            MakeUser("sewing3@crm.com", "Sewing@123", "Sewing", "3", "0923102003"),
        }, RoleNames.SewingStaff);

        await AddUsers(new[] {
            MakeUser("printing1@crm.com", "Printing@123", "Printing", "1", "0923103001"),
            MakeUser("printing2@crm.com", "Printing@123", "Printing", "2", "0923103002"),
        }, RoleNames.PrintingStaff);

        await AddUsers(new[] {
            MakeUser("finishing1@crm.com", "Finishing@123", "Finishing", "1", "0923104001"),
            MakeUser("finishing2@crm.com", "Finishing@123", "Finishing", "2", "0923104002"),
        }, RoleNames.FinishingStaff);

        await AddUsers(new[] {
            MakeUser("packaging1@crm.com", "Packaging@123", "Packaging", "1", "0923105001"),
            MakeUser("packaging2@crm.com", "Packaging@123", "Packaging", "2", "0923105002"),
        }, RoleNames.PackagingStaff);

        await AddUser(MakeUser("quality.manager@crm.com",   "Manager@123", "Pham",  "Chat Luong", "0934000001"), RoleNames.QualityManager);
        await AddUsers(new[] {
            MakeUser("qc1@crm.com", "QC@123456", "QC", "1", "0934001001"),
            MakeUser("qc2@crm.com", "QC@123456", "QC", "2", "0934001002"),
            MakeUser("qc3@crm.com", "QC@123456", "QC", "3", "0934001003"),
            MakeUser("qc4@crm.com", "QC@123456", "QC", "4", "0934001004"),
            MakeUser("qc5@crm.com", "QC@123456", "QC", "5", "0934001005"),
        }, RoleNames.QualityControl);

        await AddUser(MakeUser("delivery.manager@crm.com",  "Manager@123", "Vo",    "Giao Hang",  "0945000001"), RoleNames.DeliveryManager);
        await AddUsers(new[] {
            MakeUser("delivery1@crm.com", "Delivery@123", "Ship", "1", "0945001001"),
            MakeUser("delivery2@crm.com", "Delivery@123", "Ship", "2", "0945001002"),
            MakeUser("delivery3@crm.com", "Delivery@123", "Ship", "3", "0945001003"),
            MakeUser("delivery4@crm.com", "Delivery@123", "Ship", "4", "0945001004"),
            MakeUser("delivery5@crm.com", "Delivery@123", "Ship", "5", "0945001005"),
        }, RoleNames.DeliveryStaff);

        await AddUser(MakeUser("design.manager@crm.com",    "Manager@123", "Minh",  "Tien",       "0956000001"), RoleNames.DesignManager);
        await AddUsers(new[] {
            MakeUser("designer1@crm.com", "Design@123", "Design", "1", "0956001001"),
            MakeUser("designer2@crm.com", "Design@123", "Design", "2", "0956001002"),
            MakeUser("designer3@crm.com", "Design@123", "Design", "3", "0956001003"),
            MakeUser("designer4@crm.com", "Design@123", "Design", "4", "0956001004"),
            MakeUser("designer5@crm.com", "Design@123", "Design", "5", "0956001005"),
        }, RoleNames.Designer);
    }

    // Idempotent: ensure Content roles + sample users exist on existing DBs
    // that were created before the AddContentStaffRole / AddContentManagerRole migrations.
    private static async Task EnsureContentStaffAsync(CrmDbContext context)
    {
        var contentStaffRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.ContentStaff);
        if (contentStaffRole == null)
        {
            contentStaffRole = new Role
            {
                Id = Guid.Parse("13131313-1313-1313-1313-131313131313"),
                Name = RoleNames.ContentStaff,
                Description = "Nhân viên content (giao việc cho design)",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            context.Roles.Add(contentStaffRole);
            await context.SaveChangesAsync();
        }

        var contentManagerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.ContentManager);
        if (contentManagerRole == null)
        {
            contentManagerRole = new Role
            {
                Id = Guid.Parse("14141414-1414-1414-1414-141414141414"),
                Name = RoleNames.ContentManager,
                Description = "Trưởng phòng content",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            context.Roles.Add(contentManagerRole);
            await context.SaveChangesAsync();
        }

        var sampleEmails = new[] { "content.manager@crm.com", "content1@crm.com", "content2@crm.com" };
        var existing = await context.Users.Where(u => sampleEmails.Contains(u.Email)).Select(u => u.Email).ToListAsync();

        async Task AddIfMissing(string email, User user, Role role)
        {
            if (existing.Contains(email)) return;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        await AddIfMissing("content.manager@crm.com", MakeUser("content.manager@crm.com", "Manager@123", "Content", "Manager", "0967000001"), contentManagerRole);
        await AddIfMissing("content1@crm.com", MakeUser("content1@crm.com", "Content@123", "Content", "1", "0967001001"), contentStaffRole);
        await AddIfMissing("content2@crm.com", MakeUser("content2@crm.com", "Content@123", "Content", "2", "0967001002"), contentStaffRole);
    }

    private static async Task SeedSampleDataAsync(CrmDbContext context)
    {
        if (await context.Customers.AnyAsync()) return;

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@dongphucbonmua.com");
        if (admin == null) return;

        var stages = await context.DealStages.ToListAsync();
        if (stages.Count == 0) return;

        var leadStage    = stages.FirstOrDefault(s => s.Name == "Tiềm năng");
        var quoteStage   = stages.FirstOrDefault(s => s.Name == "Báo giá");
        var sampleStage  = stages.FirstOrDefault(s => s.Name == "Duyệt mẫu");
        var confirmStage = stages.FirstOrDefault(s => s.Name == "Xác nhận đơn");
        var wonStage     = stages.FirstOrDefault(s => s.Name == "Hoàn thành");
        var lostStage    = stages.FirstOrDefault(s => s.Name == "Đã hủy");

        if (leadStage == null || quoteStage == null || wonStage == null || lostStage == null) return;

        var customers = new List<Customer>
        {
            new() { Name = "Nguyen Van An",  Email = "an@techviet.com",    Phone = "0912345678", CompanyName = "Cong ty Tech Viet",     Industry = "Cong nghe", Address = "123 Le Loi, Q1",      City = "Ho Chi Minh", Country = "Viet Nam", CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { Name = "Tran Thi Bich",  Email = "bich@abcschool.vn",  Phone = "0987654321", CompanyName = "Truong Tieu hoc ABC",   Industry = "Giao duc",  Address = "456 Nguyen Trai, Q5", City = "Ho Chi Minh", Country = "Viet Nam", CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new() { Name = "Le Hoang Minh",  Email = "minh@sunhotel.vn",   Phone = "0909123456", CompanyName = "Sun Hotel Group",       Industry = "Khach san", Address = "789 Hai Ba Trung, Q3",City = "Ho Chi Minh", Country = "Viet Nam", CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { Name = "Pham Duc Huy",   Email = "huy@greenfood.vn",   Phone = "0918765432", CompanyName = "Green Food JSC",        Industry = "Thuc pham", Address = "321 CMT8, Q10",        City = "Ho Chi Minh", Country = "Viet Nam", CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { Name = "Vo Thi Mai",     Email = "mai@beautyplus.vn",  Phone = "0932456789", CompanyName = "Beauty Plus Spa",       Industry = "Lam dep",   Address = "567 Vo Van Tan, Q3",   City = "Ho Chi Minh", Country = "Viet Nam", CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { Name = "Hoang Van Tuan", Email = "tuan@saigonbank.vn", Phone = "0945678901", CompanyName = "Ngan hang Sai Gon",    Industry = "Ngan hang", Address = "100 Dong Khoi, Q1",    City = "Ho Chi Minh", Country = "Viet Nam", CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { Name = "Dang Thi Lan",   Email = "lan@medcare.vn",     Phone = "0956789012", CompanyName = "Benh vien MedCare",     Industry = "Y te",      Address = "200 CMT8, Q3",         City = "Ho Chi Minh", Country = "Viet Nam", CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Name = "Bui Van Khanh",  Email = "khanh@fastlogistic.vn", Phone = "0967890123", CompanyName = "Fast Logistics",   Industry = "Van tai",   Address = "88 Truong Chinh, TB",  City = "Ho Chi Minh", Country = "Viet Nam", CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
        };
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        var deals = new List<Deal>
        {
            new() { Title = "Dong phuc IT - Tech Viet",       Value = 75000000,  CustomerId = customers[0].Id, StageId = confirmStage!.Id, CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, ExpectedCloseDate = DateTime.UtcNow.AddDays(15),  Probability = 75,  CreatedAt = DateTime.UtcNow.AddDays(-28) },
            new() { Title = "Dong phuc hoc sinh - ABC",       Value = 250000000, CustomerId = customers[1].Id, StageId = quoteStage.Id,    CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, ExpectedCloseDate = DateTime.UtcNow.AddDays(30),  Probability = 50,  CreatedAt = DateTime.UtcNow.AddDays(-23) },
            new() { Title = "Dong phuc khach san - Sun",      Value = 180000000, CustomerId = customers[2].Id, StageId = wonStage.Id,      CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, ExpectedCloseDate = DateTime.UtcNow.AddDays(-5),  Probability = 100, ActualCloseDate = DateTime.UtcNow.AddDays(-5), CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { Title = "Dong phuc nha may - Green Food", Value = 120000000, CustomerId = customers[3].Id, StageId = sampleStage!.Id,  CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, ExpectedCloseDate = DateTime.UtcNow.AddDays(45),  Probability = 25,  CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new() { Title = "Dong phuc spa - Beauty Plus",    Value = 35000000,  CustomerId = customers[4].Id, StageId = leadStage.Id,     CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, ExpectedCloseDate = DateTime.UtcNow.AddDays(60),  Probability = 10,  CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new() { Title = "Dong phuc ngan hang - SGB",      Value = 500000000, CustomerId = customers[5].Id, StageId = quoteStage.Id,    CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, ExpectedCloseDate = DateTime.UtcNow.AddDays(20),  Probability = 50,  CreatedAt = DateTime.UtcNow.AddDays(-4) },
            new() { Title = "Dong phuc y te - MedCare",       Value = 90000000,  CustomerId = customers[6].Id, StageId = lostStage.Id,     CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, ExpectedCloseDate = DateTime.UtcNow.AddDays(-10), Probability = 0,   LostReason = "Khach chon doi thu", ActualCloseDate = DateTime.UtcNow.AddDays(-2), CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Title = "Dong phuc van tai - Fast",       Value = 95000000,  CustomerId = customers[7].Id, StageId = leadStage.Id,     CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, ExpectedCloseDate = DateTime.UtcNow.AddDays(50),  Probability = 10,  CreatedAt = DateTime.UtcNow },
        };
        context.Deals.AddRange(deals);
        await context.SaveChangesAsync();

        var tasks = new List<TaskItem>
        {
            new() { Title = "Xac nhan don hang Tech Viet",  DueDate = DateTime.UtcNow.AddDays(2), Priority = TaskPriority.High,   Status = CRM.Core.Enums.TaskStatus.InProgress, CustomerId = customers[0].Id, DealId = deals[0].Id, CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Title = "Gui bao gia Truong ABC",        DueDate = DateTime.UtcNow.AddDays(3), Priority = TaskPriority.High,   Status = CRM.Core.Enums.TaskStatus.Pending,    CustomerId = customers[1].Id, DealId = deals[1].Id, CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow },
            new() { Title = "Chuan bi hang giao Sun Hotel",  DueDate = DateTime.UtcNow.AddDays(7), Priority = TaskPriority.Medium, Status = CRM.Core.Enums.TaskStatus.Pending,    CustomerId = customers[2].Id, DealId = deals[2].Id, CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Title = "Gui mau vat lieu Green Food",   DueDate = DateTime.UtcNow.AddDays(1), Priority = TaskPriority.Medium, Status = CRM.Core.Enums.TaskStatus.Completed,  CustomerId = customers[3].Id, DealId = deals[3].Id, CreatedByUserId = admin.Id, AssignedToUserId = admin.Id, CompletedAt = DateTime.UtcNow.AddHours(-2), CreatedAt = DateTime.UtcNow.AddDays(-5) },
        };
        context.Tasks.AddRange(tasks);
    }

    private static async Task SeedColorFabricsAsync(CrmDbContext context)
    {
        if (await context.ColorFabrics.AnyAsync()) return;

        var colorFabrics = new List<ColorFabric>
        {
            new() { Name = "Trắng",           Description = "Màu trắng cơ bản" },
            new() { Name = "Đen",             Description = "Màu đen cơ bản" },
            new() { Name = "Xanh Dương",      Description = "Màu xanh dương đậm" },
            new() { Name = "Xanh Dương Nhạt", Description = "Màu xanh dương nhạt" },
            new() { Name = "Xanh Lá",         Description = "Màu xanh lá cây" },
            new() { Name = "Đỏ",              Description = "Màu đỏ tươi" },
            new() { Name = "Vàng",            Description = "Màu vàng" },
            new() { Name = "Cam",             Description = "Màu cam" },
            new() { Name = "Tím",             Description = "Màu tím" },
            new() { Name = "Hồng",            Description = "Màu hồng" },
            new() { Name = "Xám",             Description = "Màu xám" },
            new() { Name = "Nâu",             Description = "Màu nâu" },
            new() { Name = "Be/Kem",          Description = "Màu be/kem" },
            new() { Name = "Xanh Rêu",        Description = "Màu xanh rêu" },
            new() { Name = "Đỏ Đô",           Description = "Màu đỏ đô/burgundy" }
        };
        context.ColorFabrics.AddRange(colorFabrics);
    }

    private static async Task SeedShirtComponentsAsync(CrmDbContext context)
    {
        if (await context.ShirtComponents.AnyAsync()) return;

        var colorFabrics = await context.ColorFabrics.ToListAsync();
        var white = colorFabrics.FirstOrDefault(cf => cf.Name == "Trắng");

        var components = new List<ShirtComponent>
        {
            new() { Name = "Cổ Tròn",    Type = ComponentType.Collar, ImageUrl = "/images/components/collar-round.png" },
            new() { Name = "Cổ Đức",     Type = ComponentType.Collar, ImageUrl = "/images/components/collar-pointed.png" },
            new() { Name = "Cổ Trụ",     Type = ComponentType.Collar, ImageUrl = "/images/components/collar-mandarin.png" },
            new() { Name = "Cổ Bầu",     Type = ComponentType.Collar, ImageUrl = "/images/components/collar-club.png" },
            new() { Name = "Cổ Tim",     Type = ComponentType.Collar, ImageUrl = "/images/components/collar-v.png" },
            new() { Name = "Tay Ngắn",   Type = ComponentType.Sleeve, ImageUrl = "/images/components/sleeve-short.png" },
            new() { Name = "Tay Dài",    Type = ComponentType.Sleeve, ImageUrl = "/images/components/sleeve-long.png" },
            new() { Name = "Tay Raglan", Type = ComponentType.Sleeve, ImageUrl = "/images/components/sleeve-raglan.png" },
            new() { Name = "Tay Phồng",  Type = ComponentType.Sleeve, ImageUrl = "/images/components/sleeve-puff.png" },
            new() { Name = "Nút Trắng",    Type = ComponentType.Button },
            new() { Name = "Nút Đen",      Type = ComponentType.Button },
            new() { Name = "Nút Xanh",     Type = ComponentType.Button },
            new() { Name = "Nút Kim Loại", Type = ComponentType.Button },
            new() { Name = "Cotton 100%",                     Type = ComponentType.Fabric, ColorFabricId = white?.Id },
            new() { Name = "Cotton Pha",                      Type = ComponentType.Fabric, ColorFabricId = white?.Id },
            new() { Name = "TC (65% Polyester, 35% Cotton)",  Type = ComponentType.Fabric },
            new() { Name = "Kate",                            Type = ComponentType.Fabric },
            new() { Name = "Thun Cá Sấu",                     Type = ComponentType.Fabric },
            new() { Name = "Thun Cotton",                     Type = ComponentType.Fabric },
            new() { Name = "4Scoolmate",                      Type = ComponentType.Fabric },
            new() { Name = "Ôm (Slim Fit)",     Type = ComponentType.Body },
            new() { Name = "Vừa (Regular Fit)", Type = ComponentType.Body },
            new() { Name = "Rộng (Loose Fit)",  Type = ComponentType.Body },
            new() { Name = "Không Sọc", Type = ComponentType.Stripe },
            new() { Name = "Sọc Dọc",   Type = ComponentType.Stripe },
            new() { Name = "Sọc Ngang", Type = ComponentType.Stripe },
            new() { Name = "Sọc Chéo",  Type = ComponentType.Stripe },
            new() { Name = "Viền Cổ Đơn",  Type = ComponentType.CollarStripe },
            new() { Name = "Viền Cổ Đôi",  Type = ComponentType.CollarStripe },
            new() { Name = "Không Viền Cổ", Type = ComponentType.CollarStripe }
        };
        context.ShirtComponents.AddRange(components);
    }

    private static async Task SeedProductionStagesAsync(CrmDbContext context)
    {
        if (await context.ProductionStages.AnyAsync()) return;

        var stages = new List<ProductionStage>
        {
            new() { StageOrder = 1, StageName = "Cắt vải",                        Description = "Cắt vải theo đúng size và số lượng",           ResponsibleRole = RoleNames.CuttingStaff,   IsActive = true },
            new() { StageOrder = 2, StageName = "May",                             Description = "May thành phẩm theo kiểu dáng yêu cầu",         ResponsibleRole = RoleNames.SewingStaff,    IsActive = true },
            new() { StageOrder = 3, StageName = "In / Thêu logo",                 Description = "In hoặc thêu logo, tên riêng theo thiết kế",     ResponsibleRole = RoleNames.PrintingStaff,  IsActive = true },
            new() { StageOrder = 4, StageName = "Hoàn thiện (vệ sinh, cắt chỉ)", Description = "Vệ sinh sản phẩm, cắt chỉ thừa, là phẳng",       ResponsibleRole = RoleNames.FinishingStaff, IsActive = true },
            new() { StageOrder = 5, StageName = "Kiểm tra chất lượng",            Description = "Kiểm tra chất lượng trước khi đóng gói",         ResponsibleRole = RoleNames.QualityControl, IsActive = true },
            new() { StageOrder = 6, StageName = "Đóng gói",                       Description = "Đóng gói sản phẩm, chuẩn bị giao hàng",          ResponsibleRole = RoleNames.PackagingStaff, IsActive = true },
        };
        context.ProductionStages.AddRange(stages);
    }

    private static async Task SeedLookupsAsync(CrmDbContext context)
    {
        // ── ProductionDaysOptions ─────────────────────────────────────
        if (!await context.ProductionDaysOptions.AnyAsync())
        {
            context.ProductionDaysOptions.AddRange(
                new ProductionDaysOption { Name = "Nhanh (7 ngày)",      Days = 7,  IsActive = true },
                new ProductionDaysOption { Name = "Tiêu chuẩn (15 ngày)", Days = 15, IsActive = true },
                new ProductionDaysOption { Name = "Thường (20 ngày)",    Days = 20, IsActive = true },
                new ProductionDaysOption { Name = "Chậm (30 ngày)",      Days = 30, IsActive = true }
            );
        }

        // ── Materials ─────────────────────────────────────────────────
        if (!await context.Materials.AnyAsync())
        {
            context.Materials.AddRange(
                new Material { Name = "Cotton 100%",                    IsActive = true },
                new Material { Name = "Cotton pha (CVC)",               IsActive = true },
                new Material { Name = "TC (65% Polyester, 35% Cotton)", IsActive = true },
                new Material { Name = "Kate",                           IsActive = true },
                new Material { Name = "Thun cá sấu",                    IsActive = true },
                new Material { Name = "Thun cotton",                    IsActive = true },
                new Material { Name = "Mè bóng",                        IsActive = true },
                new Material { Name = "Coolmate",                       IsActive = true }
            );
        }

        // ── ProductForms (dáng áo) ────────────────────────────────────
        if (!await context.ProductForms.AnyAsync())
        {
            context.ProductForms.AddRange(
                new ProductForm { Name = "Ôm (Slim Fit)",     IsActive = true },
                new ProductForm { Name = "Vừa (Regular Fit)", IsActive = true },
                new ProductForm { Name = "Rộng (Loose Fit)",  IsActive = true }
            );
        }

        // ── ProductSpecifications (cổ / tay / chi tiết) ───────────────
        if (!await context.ProductSpecifications.AnyAsync())
        {
            context.ProductSpecifications.AddRange(
                new ProductSpecification { Name = "Cổ tròn, tay ngắn",       IsActive = true },
                new ProductSpecification { Name = "Cổ tròn, tay dài",        IsActive = true },
                new ProductSpecification { Name = "Cổ bẻ (polo), tay ngắn",  IsActive = true },
                new ProductSpecification { Name = "Cổ bẻ (polo), tay dài",   IsActive = true },
                new ProductSpecification { Name = "Cổ tim, tay ngắn",        IsActive = true },
                new ProductSpecification { Name = "Cổ trụ, tay ngắn",        IsActive = true }
            );
        }

        await context.SaveChangesAsync();

        // ── Collections + links (phụ thuộc vào các lookup trên) ───────
        if (!await context.Collections.AnyAsync())
        {
            var materials    = await context.Materials.ToListAsync();
            var forms        = await context.ProductForms.ToListAsync();
            var specs        = await context.ProductSpecifications.ToListAsync();
            var colorFabrics = await context.ColorFabrics.ToListAsync();

            Guid? Mid(string n) => materials.FirstOrDefault(x => x.Name == n)?.Id;
            Guid? Fid(string n) => forms.FirstOrDefault(x => x.Name == n)?.Id;
            Guid? Sid(string n) => specs.FirstOrDefault(x => x.Name == n)?.Id;
            Guid? Cid(string n) => colorFabrics.FirstOrDefault(x => x.Name == n)?.Id;

            void AddCollection(string name, string desc,
                                string[] matNames, string[] formNames, string[] specNames, string[] colorNames)
            {
                var col = new Collection { Name = name, Description = desc, IsActive = true };
                context.Collections.Add(col);
                context.SaveChanges();

                foreach (var m in matNames) { var id = Mid(m); if (id.HasValue)
                    context.Set<CollectionMaterial>().Add(new CollectionMaterial { CollectionId = col.Id, MaterialId = id.Value }); }
                foreach (var f in formNames) { var id = Fid(f); if (id.HasValue)
                    context.Set<CollectionForm>().Add(new CollectionForm { CollectionId = col.Id, ProductFormId = id.Value }); }
                foreach (var s in specNames) { var id = Sid(s); if (id.HasValue)
                    context.Set<CollectionSpecification>().Add(new CollectionSpecification { CollectionId = col.Id, ProductSpecificationId = id.Value }); }
                foreach (var c in colorNames) { var id = Cid(c); if (id.HasValue)
                    context.Set<CollectionColor>().Add(new CollectionColor { CollectionId = col.Id, ColorFabricId = id.Value }); }
                context.SaveChanges();
            }

            AddCollection(
                "Bộ sưu tập cổ bẻ",
                "Áo polo cổ bẻ, đa dạng chất liệu và màu sắc",
                new[] { "Cotton 100%", "Cotton pha (CVC)", "Thun cá sấu" },
                new[] { "Ôm (Slim Fit)", "Vừa (Regular Fit)" },
                new[] { "Cổ bẻ (polo), tay ngắn", "Cổ bẻ (polo), tay dài" },
                new[] { "Trắng", "Đen", "Xanh Dương", "Xanh Lá", "Đỏ" });

            AddCollection(
                "Bộ sưu tập cổ dệt",
                "Áo polo cổ dệt cao cấp",
                new[] { "Cotton 100%", "Thun cá sấu", "Coolmate" },
                new[] { "Vừa (Regular Fit)" },
                new[] { "Cổ bẻ (polo), tay ngắn" },
                new[] { "Trắng", "Đen", "Xanh Lá", "Đỏ Đô", "Xanh Rêu" });

            AddCollection(
                "Bộ sưu tập cổ tròn",
                "Áo thun cổ tròn căn bản",
                new[] { "Cotton 100%", "TC (65% Polyester, 35% Cotton)", "Thun cotton" },
                new[] { "Ôm (Slim Fit)", "Vừa (Regular Fit)", "Rộng (Loose Fit)" },
                new[] { "Cổ tròn, tay ngắn", "Cổ tròn, tay dài" },
                new[] { "Trắng", "Đen", "Xám", "Xanh Dương", "Đỏ", "Vàng" });

            AddCollection(
                "Bộ sưu tập đồng phục công sở",
                "Áo sơ mi công sở, kate cao cấp",
                new[] { "Kate" },
                new[] { "Ôm (Slim Fit)", "Vừa (Regular Fit)" },
                new[] { "Cổ trụ, tay ngắn" },
                new[] { "Trắng", "Xanh Dương Nhạt", "Be/Kem" });
        }
    }
}
