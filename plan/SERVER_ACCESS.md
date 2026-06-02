# Server Access - CRM Production

CRM hiện chỉ deploy trên 1 server production.

| Brand       | IP              | Domain              | DB                   |
| ----------- | --------------- | ------------------- | -------------------- |
| xanhuniform | `64.176.83.102` | crm.xanhuniform.com | `crm_dongphucbonmua` |

Khi apply migration trên production, luôn truyền tên DB tường minh vào `psql -d <db>`. Không dựa vào regex parse `appsettings.Production.json`, vì trước đây đã từng silent-fallback sang DB `postgres` khi regex match rỗng.

## Server

- **Label**: `xanhuniform-crm`
- **IP**: `64.176.83.102`
- **Region**: Singapore
- **Domain**: https://crm.xanhuniform.com
- **OS**: Ubuntu 22.04 x64
- **vCPU/RAM/Storage**: 1 vCPU / 2 GB RAM / 55 GB SSD
- **DB**: `crm_dongphucbonmua` (Postgres, user `crmuser`)
- **SSH user**: `linuxuser`
- **GitHub Actions**: dùng secret `SSH_HOST`, `SSH_USER`, `SSH_PRIVATE_KEY`

## SSH

```bash
ssh linuxuser@64.176.83.102
ssh -i ~/.ssh/id_ed25519 linuxuser@64.176.83.102
ssh-keyscan -H 64.176.83.102 >> ~/.ssh/known_hosts
```

## Deploy Paths

- Backend (.NET publish output): `/var/www/crm-api/`
- Frontend (Angular dist): `/var/www/crm-web/`
- Systemd unit: `crm-api`
- Backend listen port behind nginx: `127.0.0.1:5000`

## Quick Diagnostics

```bash
sudo journalctl -u crm-api -n 200 --no-pager
sudo journalctl -u crm-api -f
sudo systemctl status crm-api --no-pager
sudo systemctl restart crm-api

curl -i http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"smoke","password":"test"}'

curl -i https://crm.xanhuniform.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"smoke","password":"test"}'

sudo nginx -t
cat /etc/nginx/sites-enabled/crm
sudo journalctl -u nginx -n 100 --no-pager

sudo -u postgres psql -d crm_dongphucbonmua -c "\dt"
sudo -u postgres psql -d crm_dongphucbonmua -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY 1 DESC LIMIT 10;"
```

## Deploy Failure Checklist

1. SSH vào server.
2. Chạy `sudo journalctl -u crm-api -n 200 --no-pager`.
3. Nếu là lỗi migration, kiểm tra `__EFMigrationsHistory`.
4. Nếu là lỗi seed location, kiểm tra `/var/www/crm-api/Data/Seeds/`.
5. Sau khi fix, chạy `sudo systemctl restart crm-api && sudo journalctl -u crm-api -f`.

## Migration Notes

Nếu cần apply migration thủ công:

```bash
sudo systemctl stop crm-api
sudo -u postgres psql -d crm_dongphucbonmua -v ON_ERROR_STOP=1 -f /tmp/fix-migrations.sql
sudo systemctl start crm-api
```

Đảm bảo `crmuser` có quyền trên schema public:

```sql
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO crmuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO crmuser;
GRANT ALL PRIVILEGES ON SCHEMA public TO crmuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO crmuser;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO crmuser;
```
