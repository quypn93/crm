# Server Access — CRM Production

## Thông tin server (Vultr)
- **Label**: `xanhuniform-crm`
- **IP**: `64.176.83.102`
- **Region**: Singapore
- **Domain**: https://crm.xanhuniform.com
- **OS**: Ubuntu 22.04 x64
- **vCPU/RAM/Storage**: 1 vCPU / 2 GB RAM / 55 GB SSD
- **SSH user**: `linuxuser`
- **Auth**: password (xem mật khẩu ở trang Vultr → Overview → Password) hoặc SSH key đã add cho `linuxuser`
- **GitHub Actions**: dùng secret `SSH_USER` + `SSH_PRIVATE_KEY` (cùng `linuxuser` này).

## Kết nối SSH

```bash
# Dùng password (sẽ prompt)
ssh linuxuser@64.176.83.102

# Hoặc dùng SSH key đã add cho linuxuser
ssh -i ~/.ssh/id_ed25519 linuxuser@64.176.83.102

# Lần đầu — bỏ qua prompt confirm fingerprint
ssh-keyscan -H 64.176.83.102 >> ~/.ssh/known_hosts
ssh linuxuser@64.176.83.102
```

> `linuxuser` thường là sudoer trên Vultr Ubuntu image — các lệnh `sudo systemctl ...` bên dưới sẽ prompt password lần đầu trong session.

## Thư mục triển khai
- Backend (.NET publish output): `/var/www/crm-api/`
- Frontend (Angular dist): `/var/www/crm-web/`
- Systemd unit: `crm-api`

## Lệnh chẩn đoán nhanh

```bash
# Xem log backend (200 dòng gần nhất)
sudo journalctl -u crm-api -n 200 --no-pager

# Xem log realtime
sudo journalctl -u crm-api -f

# Trạng thái service
sudo systemctl status crm-api --no-pager

# Restart thủ công
sudo systemctl restart crm-api

# Test API trực tiếp (bypass nginx)
curl -i http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"smoke","password":"test"}'

# Test qua nginx
curl -i https://crm.xanhuniform.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"smoke","password":"test"}'

# Cấu hình nginx
sudo nginx -t
cat /etc/nginx/sites-enabled/crm
sudo journalctl -u nginx -n 100 --no-pager

# PostgreSQL
sudo -u postgres psql -d crm_dongphucbonmua -c "\dt"
sudo -u postgres psql -d crm_dongphucbonmua -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY 1 DESC LIMIT 10;"
```

## Quy trình debug khi smoke test fail (502)

1. SSH vào server.
2. `sudo journalctl -u crm-api -n 200 --no-pager` — tìm exception/stack trace ở khoảng thời gian deploy.
3. Nếu là lỗi migration → kiểm tra `__EFMigrationsHistory` xem migration nào dừng giữa chừng.
4. Nếu là lỗi seeding (FileNotFound vietnam-*.json) → kiểm tra `/var/www/crm-api/Data/Seeds/` có file không.
5. Sau khi fix → `sudo systemctl restart crm-api && sudo journalctl -u crm-api -f`.

## Sự cố 2026-04-28: deploy "new feature" → API 502 (đã fix)

**Triệu chứng ban đầu**: Smoke test fail. Journal cho thấy:
```
Npgsql.PostgresException: 42P01: relation "Provinces" does not exist
   at CRM.Infrastructure.Data.DataSeeder.SeedVietnamLocationsAsync
crm-api.service: Main process exited, code=dumped, status=6/ABRT
restart counter is at 220
```

**Root cause** (gồm 2 lớp):
1. `db.Database.Migrate()` ở [Program.cs:253](backend/CRM.API/Program.cs#L253) **không apply** 6 migration mới trên prod — `__EFMigrationsHistory` chỉ ghi nhận `InitialCreate`. Lý do gốc chưa rõ; nghi ngờ mismatch tool version (local `dotnet-ef 8.0.11` vs project EF 9.0.1) hoặc permission issue silent.
2. **`crmuser` không có quyền CREATE TABLE trong schema public**, và các bảng cũ vốn được tạo ban đầu khi setup bằng `postgres` rồi chuyển owner. Khi `Migrate()` cố tạo bảng mới với crmuser sẽ gặp 42501 — nhưng vì lý do (1) Migrate() không chạy nên không throw rõ.

**Cách fix đã thực hiện**:
1. Update local tool: `dotnet tool update -g dotnet-ef --version 9.0.1`.
2. Generate idempotent SQL từ local: `dotnet ef migrations script 20260420114923_InitialCreate -i -p CRM.Infrastructure -s CRM.API -o ../fix-migrations.sql`.
3. SCP file lên server `/tmp/fix-migrations.sql`.
4. **Stop service trước** (bắt buộc — RestartSec=10 sẽ tự seed lại nếu không stop):
   ```bash
   sudo systemctl stop crm-api
   ```
5. Xóa Content roles+users đã được seeder tạo dở (tránh PK conflict khi migration `AddContentStaffRole`/`AddContentManagerRole` insert lại):
   ```sql
   DELETE FROM "UserRoles" WHERE "RoleId" IN ('13131313-...','14141414-...');
   DELETE FROM "Users" WHERE "Email" IN ('content.manager@crm.com','content1@crm.com','content2@crm.com');
   DELETE FROM "Roles" WHERE "Id" IN ('13131313-...','14141414-...');
   ```
6. Apply SQL với `postgres` user (vì crmuser không có CREATE TABLE):
   ```bash
   sudo -u postgres psql -d crm_dongphucbonmua -v ON_ERROR_STOP=1 -f /tmp/fix-migrations.sql
   ```
7. **Cấp đầy đủ quyền cho `crmuser`** (đây là phần quan trọng để khỏi gặp lại):
   ```sql
   ALTER TABLE "Provinces" OWNER TO crmuser;
   ALTER TABLE "Wards" OWNER TO crmuser;
   GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO crmuser;
   GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO crmuser;
   GRANT ALL PRIVILEGES ON SCHEMA public TO crmuser;
   ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO crmuser;
   ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO crmuser;
   ```
8. `sudo systemctl start crm-api` → service up, app tự seed nốt 34 provinces + 114 wards + content roles/users.

**Bài học cho lần sau**:
- Workflow [.github/workflows/deploy.yml:69-72](.github/workflows/deploy.yml#L69-L72) "Restart backend service" mask được crash nhờ `head -15` ở cuối pipe → cần sửa để fail-fast.
- `crmuser` của prod cần có CREATE quyền trong schema public + DEFAULT PRIVILEGES, nếu không sẽ gặp lại mỗi khi có migration tạo bảng mới.
- Local `dotnet-ef` tool nên match version EF của project.

**Bước chẩn đoán DB**:
```bash
# 1. Tìm connection string thật
sudo cat /etc/systemd/system/crm-api.service
sudo cat /var/www/crm-api/appsettings.Production.json 2>/dev/null
sudo cat /var/www/crm-api/appsettings.json | grep -A1 -i connection

# 2. Vào psql (đổi DB name nếu khác)
sudo -u postgres psql -d crm_dongphucbonmua

# 3. Trong psql
\dt
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY 1;
SELECT to_regclass('public."Provinces"');   -- NULL = không tồn tại
SELECT to_regclass('public."Wards"');
\q
```

**Cách fix** (chọn 1 trong 2):

### A. Xoá history entry để EF apply lại migration (an toàn nếu các bảng/cột chưa tồn tại):
```sql
-- Lưu ý: chạy lệnh này nếu Provinces/Wards KHÔNG tồn tại trên prod
DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" IN (
  '20260423145406_AddLocationsAndOrderShipping',
  '20260423171404_AddOrderDeliveryMethod',
  '20260424030013_AddGhtkTrackingToOrder',
  '20260424053629_AddDesignAssignmentFlow',
  '20260426000000_AddContentStaffRole',
  '20260426010000_AddContentManagerRole'
);
```
**Lưu ý quan trọng**: Trước khi xóa, kiểm tra từng migration đã apply phần nào rồi (cột Orders.ShippingContactName, Orders.DesignId, Roles row 1313..., 1414...). Nếu một số ALTER đã chạy thật → chỉ xóa history của những migration mà ALTER chưa hiệu lực. Migration sẽ throw "column already exists" nếu cột thật sự đã có.

### B. Tạo bảng thủ công bằng SQL trong migration (nếu chỉ thiếu Provinces/Wards):
Chạy nguyên đoạn DDL ở `Migrations/20260423145406_AddLocationsAndOrderShipping.cs` (block CreateTable + CreateIndex) trực tiếp trong psql.

### Sau khi fix:
```bash
sudo systemctl restart crm-api
sudo journalctl -u crm-api -f
# Xác nhận hết "Provinces does not exist", thấy "Now listening on: http://localhost:5000"
curl -s -o /dev/null -w "%{http_code}\n" -X POST https://crm.xanhuniform.com/api/auth/login \
  -H "Content-Type: application/json" -d '{"email":"smoke","password":"test"}'
# Mong đợi: 401
```
