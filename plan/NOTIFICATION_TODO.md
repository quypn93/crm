# Notification System — TODO

Danh sách những việc còn lại cho hệ thống thông báo, sau khi đã hoàn tất Phase 1A → 1D.

Liên quan: [NOTIFICATION_SYSTEM.md](NOTIFICATION_SYSTEM.md) (plan gốc).

---

## 🔴 Cần xử lý sớm

### 1. Cấp SMTP credentials (Google Workspace)

Hiện đang dùng `NoOpEmailSender` (log warning, skip). Khi có account Google Workspace + App Password:

- [ ] Bật 2FA cho account Workspace dùng để gửi mail.
- [ ] Tạo App Password tại myaccount.google.com → Security → App passwords.
- [ ] Update `appsettings.Production.json` (hoặc env var):
  - `Email:Username` = full email account (vd `noreply@dongphucbonmua.com`)
  - `Email:Password` = App Password 16 ký tự
  - `Email:FromAddress` = trùng `Username` (Google reject nếu khác)
  - `Email:FromName` = "CRM Đồng Phục Bốn Mùa"
- [ ] Restart API. DI tự switch sang `SmtpEmailSender` — không cần sửa code.
- [ ] Test gửi 1 email thật: tạo task assign cho user khác → check inbox.
- [ ] Verify SPF/DKIM (Google Workspace setup wizard tự config nếu domain đã verify).

### 2. Verify end-to-end Phase 1A

Chưa test thực tế. Cần làm:

- [ ] Login 2 tab cùng user → assign task tab A → bell tab B kêu trong < 2s, badge tăng.
- [ ] Login 2 user khác nhau → user A tạo task assign cho user B → user B nhận, A không.
- [ ] Click notification trong dropdown → navigate đúng `/tasks/{id}/edit` + mark read tự động.
- [ ] "Đánh dấu tất cả đã đọc" → unread count về 0 cả 2 tab.
- [ ] Set task `DueDate` = `now + 23h` → đợi `TaskReminderHostedService` chạy (tối đa 30 phút) → verify notification `TaskDueSoon` xuất hiện, có log row trong `TaskNotificationLogs`.
- [ ] Test admin preferences page `/admin/notification-preferences`: tắt Email cho 1 role → tạo task → verify không gửi email.

### 3. Manager escalation cho TaskOverdue

Plan gốc nói: TaskOverdue notify thêm manager của role assignee (SalesRep → SalesManager, Designer → DesignManager, ContentStaff → ContentManager). Hiện tại `TaskReminderJob.BuildOverdueEvent` chỉ notify assignee.

- [ ] Thêm helper `ResolveManagerForUserAsync(Guid userId)` ở Application service (đọc role của user → map sang manager role → query 1 user thuộc manager role đó).
- [ ] Trong `TaskReminderJob`, sau khi build event cho assignee, build thêm event cho manager (cùng task, message thêm context "Nhân viên X chưa hoàn thành").
- [ ] Cẩn thận: Admin/Manager role thì không escalate (tránh self-loop).

---

## 🟡 Phase 2 — Phủ Deal events

Khi muốn mở rộng notify cho module Deal (đã có code logic, chỉ cần dispatch).

- [ ] Thêm enum `DealAssigned`, `DealStageChanged`, `DealClosed` vào `NotificationType`.
- [ ] Update `appsettings.json:Notifications:RoleDefaults` cho 3 type mới.
- [ ] Inject `INotificationDispatcher` vào `DealService`.
- [ ] Tích hợp:
  - `DealService.CreateAsync` / `UpdateAsync` (assignee changed) → `DealAssigned`
  - `DealService.UpdateStageAsync` → `DealStageChanged` cho assignee + creator
  - Khi stage = "Won" hoặc "Lost" → `DealClosed`
- [ ] Update admin preferences UI (`role-preferences-page`): thêm 3 cột type mới vào `types` array.
- [ ] Thêm i18n label trong `NOTIFICATION_TYPE_LABELS` (frontend model).

---

## 🟡 Phase 3 — Phủ Order / Production / GHTK / SePay

### 3.1 Order events
- [ ] Enum: `OrderCreated`, `OrderStatusChanged`, `OrderPaymentReceived`, `OrderDeliveryUpdated`.
- [ ] `OrderService.CreateAsync` → notify SalesManager + Admin (`OrderCreated`).
- [ ] `OrderService.UpdateStatusAsync` → notify assignee + creator (`OrderStatusChanged`).

### 3.2 Production events
- [ ] Enum: `ProductionStepBlocked`, `QcRejected`.
- [ ] `OrderProductionService` hook khi step bị block (chuyển status sang Blocked) → notify ProductionManager.
- [ ] Khi QC reject → notify stage staff + ProductionManager.

### 3.3 Webhook integrations
- [ ] GHTK webhook handler trong `GhtkController.Webhook` → khi đơn `delivered` / `returned` / `cancelled` → notify sale owner của đơn (`OrderDeliveryUpdated`).
- [ ] SePay webhook handler (nếu có) → khi nhận tiền cọc → notify sale của đơn (`OrderPaymentReceived`).

### 3.4 Cập nhật role defaults
- [ ] Mở rộng `appsettings.json:Notifications:RoleDefaults` cho ProductionManager, QualityManager, DeliveryManager, các role staff.
- [ ] Show thêm role + type trong admin preferences UI (hiện chỉ show 7 role Sale/Design/Content/Admin).

---

## 🟢 Cải tiến (low priority)

### Email templates
Hiện đang inline HTML trong `TaskService.BuildEmailHtml` và `TaskReminderJob.BuildEmailHtml`. Khi nhiều type sẽ rối.

- [ ] Tách ra file `.html` ở `CRM.Application/Templates/Email/`, copy ra output.
- [ ] Helper `IEmailTemplateRenderer` với placeholder `{{Property}}` (simple) hoặc `Scriban` (loop/condition).
- [ ] Set `<None Update="Templates/Email/*.html"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>` trong `.csproj`.
- [ ] Link CTA email phải dùng `AppSettings:FrontendBaseUrl` (đã có sẵn trong appsettings) làm prefix, không phải relative path. Hiện email link đang dùng relative `/tasks/{id}/edit` — mở email trong Gmail sẽ không click được.

### Dev-only test endpoint
- [ ] `POST /api/notifications/test` (gated bằng `app.Environment.IsDevelopment()`) — dispatch notification giả lập cho current user. Giúp dev frontend không cần trigger event thật.

### Quiet hours / Digest email
- [ ] Quiet hours: skip email từ 22h → 7h giờ VN, queue lại morning.
- [ ] Digest mode: gom Task overdue summary gửi 1 lần/ngày thay vì spam realtime.

### Privacy / Performance
- [ ] Email body không nên chứa thông tin nhạy cảm (giá đơn, full thông tin khách). Hiện chỉ có title task — OK. Khi mở rộng cần review.
- [ ] Notification storm protection: khi 1 event trigger > 20 notification (vd order broadcast), batch insert (đã làm) + rate-limit dispatcher.
- [ ] Index review khi `Notifications` table > 1M row — có thể cần partial index `WHERE IsRead = false`.

### Monitoring
- [ ] Log metric `notification_dispatched_total` + `notification_email_sent_total` ra Serilog/Prometheus (tương lai).
- [ ] Health check endpoint cho SignalR hub (`/health/signalr`).

---

## 🔵 Phase 4 — Mở rộng (sau khi production ổn định)

- [ ] **Push notification mobile** (FCM) — khi có app mobile.
- [ ] **Slack / Telegram bot** — gửi notification ra channel team (alternative cho email).
- [ ] **Hangfire** — khi backend scale > 1 instance, distributed lock + retry built-in.
- [ ] **OAuth2 cho SMTP** (thay App Password) — an toàn hơn, không cần password trong config.
- [ ] **User notification preferences** (per-user override per-role) — hiện chỉ có per-role admin-only. Nếu user request quyền tự tắt thì cần thêm bảng `NotificationUserPreference` override `NotificationRolePreference`.

---

## 🐛 Known issues / Edge cases cần verify

- [ ] **JWT trong query string** (cho SignalR) sẽ bị log ở Nginx access log. Khi deploy production cần config Nginx `log_format` strip `access_token` query param.
- [ ] **Token expire khi tab mở lâu**: SignalR `accessTokenFactory` đọc từ storage mỗi lần reconnect, nhưng nếu token expired và không có refresh logic ở realtime service → connection sẽ disconnect. Cần test scenario tab mở > 1h.
- [ ] **Multiple instance backend**: hiện 2 hosted service không có distributed lock — nếu deploy nhiều instance, job sẽ chạy song song → gửi notification trùng. Cần env var `BACKGROUND_JOBS_ENABLED=true` cho duy nhất 1 instance, HOẶC `pg_try_advisory_lock` HOẶC migrate Hangfire.
- [ ] **Frontend pagination**: `notification-list-page` chỉ hiển thị 5 page (start..end), không có "...". OK cho < 30 page, sau cần infinite scroll.
- [ ] **Bell badge sau khi delete notification chưa đọc**: đã decrement count ở `NotificationService.delete` — verify lại khi test E2E.
- [ ] **Time zone ở email**: `BuildEmailHtml` dùng `dueLocal = DueDate.ToLocalTime()` — ToLocalTime của server, không phải user. Server đặt giờ VN thì OK, nhưng nếu deploy container UTC → email hiển thị giờ UTC. Cần fix bằng `TimeZoneInfo.ConvertTimeFromUtc(dueDate, vnZone)`.

---

## ✅ Đã hoàn thành (tham khảo)

- Phase 1A — Foundation + In-app + SignalR cho Task ✅
- Phase 1B — Background jobs (TaskReminder, Cleanup) ✅
- Phase 1C — SmtpEmailSender (MailKit, auto-detect) ✅
- Phase 1D — Admin preferences UI ✅
- Migration `AddNotifications` đã apply DB ✅
- 5 task events: TaskAssigned, TaskDueSoon, TaskOverdue, TaskCompleted, TaskReassigned ✅
