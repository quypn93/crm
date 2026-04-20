# CRM - Việc cần làm tay (sau khi code)

## 1. Tích hợp SePay (cộng tiền tự động từ Techcombank)

- [ ] Đăng ký tài khoản tại [sepay.vn](https://sepay.vn)
- [ ] Trong SePay dashboard: liên kết tài khoản Techcombank (qua internet banking)
- [ ] Tạo API Key trên SePay → dán vào [`backend/CRM.API/appsettings.json`](backend/CRM.API/appsettings.json):
  ```json
  "SePay": { "ApiKey": "<paste_key_here>" }
  ```
- [ ] Restart backend
- [ ] Expose backend ra internet:
  - **Dev**: `ngrok http 5222` → URL dạng `https://xxx.ngrok-free.app`
  - **Production**: deploy lên VPS/server có domain thật
- [ ] Trong SePay dashboard → **Cài đặt webhook**:
  - URL: `https://<domain>/api/deposits/sepay-webhook`
  - Loại giao dịch: **Tiền vào**
  - Trạng thái: **Thành công**
- [ ] Test: bấm "Test webhook" trên SePay → check trang `/settings/deposits`
- [ ] Thật: chuyển 1000đ → giao dịch xuất hiện realtime

## 2. Seed / cấu hình dữ liệu ban đầu

- [x] Materials, ProductForms, ProductSpecifications, ProductionDaysOptions, Collections đã seed sample
- [ ] Login Admin vào `/settings/collections`, `/settings/materials`, ... để chỉnh sửa/thêm dữ liệu thực tế
- [ ] Tạo ProductionDaysOptions theo business thật (hiện có "Tiêu chuẩn 7 ngày", "Gấp 10 ngày")

## 3. Pin Angular dev server port (tránh phải đổi `FrontendBaseUrl` mỗi lần)

Trong [`frontend/crm-app/package.json`](frontend/crm-app/package.json):
```json
"scripts": {
  "start": "ng serve --port 4200 --host 127.0.0.1"
}
```
Rồi cập nhật [`backend/CRM.API/appsettings.json`](backend/CRM.API/appsettings.json):
```json
"AppSettings": { "FrontendBaseUrl": "http://127.0.0.1:4200" }
```

## 4. Production deployment

- [ ] Build frontend: `ng build --configuration production`
- [ ] Build backend: `dotnet publish -c Release -o out`
- [ ] Deploy backend lên VPS (IIS / nginx + kestrel / Docker)
- [ ] Config production connection string (SQL Server thật, không phải localdb)
- [ ] Config production `FrontendBaseUrl` = domain frontend thật
- [ ] CORS: hiện tại cho phép mọi `localhost`, production cần whitelist domain cụ thể — sửa [`backend/CRM.API/Program.cs`](backend/CRM.API/Program.cs) CORS policy
- [ ] HTTPS certificate (Let's Encrypt / CloudFlare)
- [ ] Static files `wwwroot/uploads/designs/` cần persist storage (hoặc chuyển sang S3/Azure Blob)

## 5. Features có thể bổ sung (không bắt buộc)

- [ ] Auto-match `DepositCode` đơn hàng với `DepositTransaction.code` khi webhook về (hiện sale phải điền tay)
- [ ] Notification khi có giao dịch mới match đơn của sale
- [ ] Report doanh thu theo sale, theo collection
- [ ] Export Excel lịch sử cộng tiền
- [ ] Search/filter trong trang `/settings/deposits`

## 6. Known issues

- Nếu port Angular dev đổi mỗi lần chạy (`ng serve` không pin port) → phải update `FrontendBaseUrl` backend + xóa `Orders.QrCodeToken/QrCodeImageBase64` để regenerate QR. Xem mục #3 để fix dứt điểm.
- Chrome HTTPS-first có thể auto-upgrade `http://localhost` → dùng `http://127.0.0.1` hoặc tắt HTTPS-first cho localhost.
- Order cũ (pre-migration `OrderCollectionFlowV2`) có `collectionId` = null → phải recreate hoặc dùng script migrate dữ liệu.
