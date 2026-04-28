# Chuẩn bị tích hợp GHTK (Giao Hàng Tiết Kiệm)

File này liệt kê những gì **cần bạn cung cấp** để Giai đoạn 3 chạy được với GHTK thật. Khi chạy mà các mục đánh dấu `[TBD]` chưa được điền, các endpoint GHTK sẽ trả lỗi `503 Service Unavailable` với thông báo "GHTK chưa cấu hình".

---

## 1. Tài khoản GHTK & Token

- [ ] **Email đăng ký GHTK (portal khách hàng):** `[TBD]`
- [ ] **Sandbox URL:** `https://dev.services.giaohangtietkiem.vn`
- [ ] **Production URL:** `https://services.giaohangtietkiem.vn`
- [ ] **Sandbox Token:** `[TBD]`
- [ ] **Production Token:** `[TBD]`
- [ ] **Partner Code** (nếu có, một số gói đại lý có mã riêng): `[TBD]`

> Lấy token tại: khachhang.giaohangtietkiem.vn → Tiện ích → API & Key.

---

## 2. Thông tin kho lấy hàng (pickup)

GHTK API v1 vẫn yêu cầu trường `pick_district` dù VN đã bỏ cấp huyện.
Phải điền tên quận/huyện **cũ** của địa chỉ kho — GHTK chưa đồng bộ toàn bộ schema mới.

- [ ] **Tên kho / cửa hàng (pick_name):** `[TBD]`
- [ ] **SĐT kho (pick_tel):** `[TBD]`
- [ ] **Địa chỉ số nhà + đường (pick_address):** `[TBD]`
- [ ] **Tỉnh/Thành phố (pick_province):** `[TBD]` — ví dụ `Thành phố Hồ Chí Minh`
- [ ] **Quận/Huyện cũ (pick_district):** `[TBD]` — ví dụ `Quận 1`
- [ ] **Phường/Xã (pick_ward):** `[TBD]` — ví dụ `Phường Bến Nghé`
- [ ] **Mã bưu chính (nếu có):** `[TBD]`

---

## 3. Thông tin người gửi hiển thị trên vận đơn

- [ ] **Tên công ty / shop hiển thị:** `[TBD]`
- [ ] **Email liên hệ (đơn lỗi / hỗ trợ):** `[TBD]`

---

## 4. Quyết định chính sách

### 4.1 Thời điểm tạo vận đơn GHTK

Chọn **một** trong 2:

- [ ] **A. Tạo ngay khi sale chọn hình thức "GHTK"** và đơn có đủ địa chỉ (chủ động tạo). Ưu: có mã vận đơn sớm để in. Nhược: nếu đơn thay đổi/huỷ sẽ phải gọi cancel GHTK.
- [ ] **B. Chỉ tạo khi đơn chuyển sang trạng thái "Sẵn sàng giao"** (mặc định đề xuất). Ưu: chắc chắn đơn đã duyệt. Nhược: sale không biết mã vận đơn tới lúc đó.

> **Mặc định code đang chạy theo phương án B.** Đổi sang A chỉ cần sửa một chỗ trong `OrderService.UpdateStatusAsync`.

### 4.2 Xử lý phí giao hàng

Chọn **một**:

- [ ] **A. Tự động tính phí GHTK qua API và ghi đè `shippingFee` của đơn** khi sale chọn GHTK. Ưu: không lệch số liệu. Nhược: sale không điều chỉnh được.
- [ ] **B. Hiển thị phí ước tính (ghi vào `GhtkFee`) nhưng sale tự nhập `shippingFee`** (mặc định đề xuất). Ưu: flexible, sale chốt giá với khách. Nhược: có thể lệch phí thực tế GHTK thu.

> **Mặc định đang là B.** `GhtkFee` chỉ lưu reference; `shippingFee` do sale kiểm soát.

### 4.3 Webhook nhận trạng thái đơn

- [ ] **Public URL nhận webhook GHTK** (production):
  - `https://<domain>/api/ghtk/webhook`
  - `[TBD]` — cần bạn đưa URL này cho GHTK để họ whitelist.
- [ ] **Webhook secret** (GHTK khuyến nghị thêm header key để xác thực): `[TBD]`

### 4.4 Dịch vụ mặc định

- [ ] **Loại dịch vụ (transport):** `road` (đường bộ) hoặc `fly` (bay). Mặc định hiện tại: `road`.
- [ ] **COD mặc định**: có thu hộ tiền hàng không?
  - [ ] Không (đơn đã cọc/chuyển khoản → `pick_money = 0`)
  - [ ] Có (giá trị đơn hàng)

### 4.5 Đóng gói

GHTK cần `pick_work_shift` (khung giờ nhận hàng) và `weight`/`value` ước tính.

- [ ] **Khung giờ nhận hàng (pick_work_shift):** `1` (8h-12h) / `2` (14h-18h) / `3` (cả ngày — mặc định).
- [ ] **Cân nặng mặc định mỗi áo (kg):** `[TBD]` — ví dụ `0.3`
- [ ] **Số cm tối đa mỗi chiều (L × W × H):** `[TBD]` — ví dụ `35 × 25 × 5`

---

## 5. appsettings.json (tham khảo — sẽ điền sau khi bạn cung cấp)

```jsonc
{
  "Ghtk": {
    "BaseUrl": "https://services.giaohangtietkiem.vn",
    "Token": "[TBD]",
    "PartnerCode": "",
    "WebhookSecret": "[TBD]",

    "Pick": {
      "Name": "[TBD]",
      "Tel": "[TBD]",
      "Address": "[TBD]",
      "Province": "[TBD]",
      "District": "[TBD]",
      "Ward": "[TBD]"
    },

    "Defaults": {
      "Transport": "road",
      "PickWorkShift": 3,
      "DefaultWeightKg": 0.3,
      "UseCod": false,
      "AutoCreateOnStatus": "ReadyToShip",   // A/B policy ở mục 4.1
      "AutoOverrideShippingFee": false       // A/B policy ở mục 4.2
    }
  }
}
```

---

## 6. Lưu ý mapping địa chỉ xã/phường

- GHTK chưa cập nhật toàn bộ danh sách xã/phường sau sáp nhập 2025.
- Khi tạo đơn, service sẽ gửi `deliver_ward_name` + `deliver_province_name` theo tên tỉnh/xã mới. Nếu GHTK trả lỗi "địa chỉ không hợp lệ", cần:
  - Fallback sang tên cũ nếu có bảng map.
  - Hoặc cho sale nhập thủ công khi tạo đơn.
- **Cần test kỹ trên sandbox** với 5-10 địa chỉ mẫu (mỗi miền) trước khi bật production.

---

## 7. Checklist trước khi go-live

- [ ] Điền đủ các mục `[TBD]` ở trên.
- [ ] Test sandbox: tính phí + tạo đơn + hủy đơn + webhook.
- [ ] Test 5+ địa chỉ thực (đủ 3 miền, có cả phường sáp nhập).
- [ ] Xác minh webhook endpoint public accessible (không bị Nginx chặn).
- [ ] Đặt giá trị production vào appsettings.Production.json hoặc env vars.
- [ ] Verify tài khoản có tiền (GHTK trừ tiền ký quỹ khi tạo đơn).
