# Hệ thống Notification (In-app + Email + Real-time)

Plan triển khai hệ thống thông báo cho CRM, bao gồm 3 kênh:

1. **In-app**: Lưu DB, hiển thị trên bell icon ở header, có badge đếm chưa đọc.
2. **Real-time**: Đẩy notification mới đến client đang online qua SignalR (không cần refresh).
3. **Email**: Gửi qua SMTP cho các sự kiện quan trọng (task assigned, đơn cần duyệt, v.v.) — user-configurable per event.

---

## 1. Quyết định đã chốt

### 1.1 SMTP gửi email — Google Workspace

Công ty dự kiến dùng email Google Workspace. Có 2 phương án xác thực:

**Phương án A — App Password (đề xuất giai đoạn 1):**
- Yêu cầu: account Google Workspace **bật 2FA**, sau đó tạo App Password tại myaccount.google.com → Security → App passwords.
- Cấu hình:
  - Host: `smtp.gmail.com`
  - Port: `587`
  - UseStartTls: `true`
  - Username: `noreply@<domain-công-ty>` (full email)
  - Password: App Password 16 ký tự (không phải password thường)
- Ưu: Setup nhanh (5 phút), code SMTP chuẩn hoạt động ngay.
- Nhược: Phụ thuộc 1 account; nếu account đó đổi password / mất 2FA thì gửi mail fail.

**Phương án B — OAuth2 (đề xuất sau khi ổn định):**
- Yêu cầu: tạo Google Cloud project, OAuth client, refresh token cho service account.
- Ưu: Không cần password trong config, an toàn hơn.
- Nhược: Setup phức tạp, MailKit cần SASL XOAUTH2 + thư viện refresh token.

> **Quyết định**: Bắt đầu với **A**. Khi nào bạn cấp credentials thì điền vào `appsettings.{Environment}.json` (KHÔNG commit vào git):
>
> - [ ] **Workspace email account dùng để gửi:** `[bạn cung cấp sau]`
> - [ ] **App Password (16 ký tự):** `[bạn cung cấp sau]`
> - [ ] **From Display Name:** `CRM Đồng Phục Bốn Mùa` (default)
> - [ ] **Reply-To (tuỳ chọn):** `[bạn cung cấp sau]`

> **Limit Google Workspace**: 2000 mail/ngày/account (vs Gmail free 500/ngày). Đủ rộng cho team nội bộ.
>
> **Lưu ý SPF/DKIM**: Vì gửi từ domain công ty qua Google → Google đã tự ký DKIM nếu domain được verify trong Workspace admin. SPF record của domain phải có `include:_spf.google.com`. Khi setup Workspace ban đầu nếu làm theo wizard thì 2 cái này đã đúng.

### 1.2 Phạm vi sự kiện — Giai đoạn 1: Task only

**Target roles**: Sale (`SalesManager`, `SalesRep`), Design (`DesignManager`, `Designer`), Content (`ContentManager`, `ContentStaff`).

**Sự kiện sẽ trigger notification:**

| Event | Severity | Recipient | Channel |
|---|---|---|---|
| `TaskAssigned` | info | `AssignedToUserId` (sale/design/content) | In-app + Email |
| `TaskDueSoon` (DueDate còn ≤ 24h, chưa Completed) | warning | `AssignedToUserId` | In-app + Email |
| `TaskOverdue` (DueDate < now, chưa Completed) | error | `AssignedToUserId` + manager của role tương ứng | In-app + Email (real-time) |
| `TaskCompleted` | info | `CreatedByUserId` (người giao việc) | In-app only |
| `TaskReassigned` | info | new assignee + old assignee | In-app only |

> **"Manager của role tương ứng"** logic:
> - Assignee role là `SalesRep` → notify thêm `SalesManager`
> - Assignee role là `Designer` → notify thêm `DesignManager`
> - Assignee role là `ContentStaff` → notify thêm `ContentManager`
> - Assignee role là `Admin` hoặc các role manager → không escalate.
>
> Nếu user có nhiều role → resolve manager theo role "ưu tiên cao" (hiện đơn giản dùng role đầu tiên match).

**Không thuộc phạm vi giai đoạn 1**: Deal, Order, Production, GHTK, SePay, Customer assignment. Sẽ làm sau khi giai đoạn 1 chạy ổn (ghi nhận trong roadmap mục 7).

**Không thuộc phạm vi**: notify ra khách hàng (SMS/email cuối) — đây là feature riêng, không nằm trong plan này.

### 1.3 Preferences — Admin-only, per-role

Bỏ idea per-user preferences. Thay bằng **per-role notification config** trong DB, có UI cho Admin:

```
NotificationRolePreferences
├── RoleId        FK → Roles.Id
├── Type          NotificationType
├── InApp         bool (default true)
├── Email         bool (default theo bảng 1.2)
└── UNIQUE (RoleId, Type)
```

- User KHÔNG có quyền tự bật/tắt — chỉ Admin chỉnh ở `/admin/notifications/preferences`.
- Khi user thuộc nhiều role → **OR logic**: chỉ cần 1 role bật InApp/Email là user nhận. (Tránh case role A bật, role B tắt → user không nhận được loại nào).
- Khi không có row trong DB cho `(RoleId, Type)` → fallback default từ `appsettings.json` (giống pattern Workspace settings).

### 1.4 Tần suất email — Real-time toàn bộ

- Tất cả 3 loại task email (`Assigned`, `DueSoon`, `Overdue`) gửi **real-time**, ngay khi event xảy ra.
- Không có digest / quiet hours ở giai đoạn 1.
- Tránh spam: mỗi `(TaskId, NotificationType)` chỉ gửi **1 lần** — track bằng cột `LastNotifiedAt` + `LastNotificationType` trên `TaskItem`, hoặc bảng `TaskNotificationLog` (xem mục 3.6 để chốt).

### 1.5 Retention & Background job — Đề xuất

**Retention: 30 ngày** sau khi `IsRead = true`. Notification chưa đọc thì giữ lâu hơn (90 ngày) để user không bị mất khi nghỉ phép dài. Lý do:
- 30 ngày đủ cho user xem lại lịch sử trong 1 tháng.
- Bảng notification có thể phình rất nhanh (mỗi user × event/ngày × N user).
- Giảm chi phí storage + index.

**Background job: `IHostedService` + `PeriodicTimer`** — built-in .NET, không thêm dependency. Lý do:
- Hiện tại deploy 1 instance backend (đọc Program.cs + plan deploy).
- 3 jobs cần chạy: Cleanup (mỗi 6h), TaskDueSoonScan (mỗi 30 phút), TaskOverdueScan (mỗi 30 phút).
- Khi sau này scale → migrate sang Hangfire (1 ngày công).

> Nếu muốn sau này dễ migrate, viết job logic vào service riêng (`ITaskReminderJob`), `IHostedService` chỉ là "scheduler shell" gọi service. Khi đổi sang Hangfire chỉ thay scheduler shell, business logic giữ nguyên.

---

## 2. Domain Model

### 2.1 Bảng `Notifications`

```
Id              GUID PK
RecipientUserId GUID FK → Users.Id   -- người nhận
Type            VARCHAR(64)          -- enum NotificationType (xem 2.2)
Title           VARCHAR(200)         -- tiêu đề ngắn
Message         VARCHAR(1000)        -- nội dung hiển thị
Link            VARCHAR(500) NULL    -- URL frontend, ví dụ /orders/{id}
EntityType      VARCHAR(64) NULL     -- "Order" | "Task" | "Deal" | ...
EntityId        GUID NULL            -- ref đến entity gốc
Severity        VARCHAR(16)          -- "info" | "success" | "warning" | "error"
IsRead          BOOLEAN  DEFAULT false
ReadAt          TIMESTAMP NULL
CreatedAt       TIMESTAMP            -- inherit BaseEntity
UpdatedAt       TIMESTAMP NULL

INDEX (RecipientUserId, IsRead, CreatedAt DESC)
INDEX (CreatedAt) -- cho retention cleanup
```

> **Chú ý**: PostgreSQL — dùng `timestamp without time zone` UTC. Lưu thông tin chi tiết theo dạng JSON nếu sau này cần (`Metadata jsonb NULL`) — hiện chưa cần, để mở rộng sau.

### 2.2 Enum `NotificationType` (giai đoạn 1)

```csharp
namespace CRM.Core.Enums;

public enum NotificationType
{
    // Task — phase 1
    TaskAssigned,
    TaskDueSoon,
    TaskOverdue,
    TaskCompleted,
    TaskReassigned,

    // Sẽ thêm ở các phase sau:
    // DealAssigned, DealStageChanged, DealClosed,
    // OrderCreated, OrderStatusChanged, OrderPaymentReceived, OrderDeliveryUpdated,
    // ProductionStepBlocked, ProductionStepFailed, QcRejected,
    // UserCreated, Generic
}
```

### 2.3 Bảng `NotificationRolePreferences` (admin-managed)

```
Id          GUID PK
RoleId      GUID FK → Roles.Id
Type        VARCHAR(64)        -- NotificationType
InApp       BOOLEAN  DEFAULT true
Email       BOOLEAN  DEFAULT false
CreatedAt   TIMESTAMP
UpdatedAt   TIMESTAMP

UNIQUE (RoleId, Type)
```

> Khi không có row cho `(RoleId, Type)` → fallback về **default config** (file `appsettings.json` section `Notifications:RoleDefaults`).
>
> User thuộc nhiều role → resolve theo OR logic: notify nếu **bất kỳ** role nào của user có `InApp=true` (hoặc `Email=true`).

### 2.4 Bảng `TaskNotificationLog` (chống gửi trùng)

Tránh `TaskReminderService` gửi nhiều notification trùng cho cùng 1 task:

```
Id          GUID PK
TaskId      GUID FK → Tasks.Id
Type        VARCHAR(64)        -- TaskDueSoon | TaskOverdue
SentAt      TIMESTAMP

UNIQUE (TaskId, Type)
```

> Khi `Task.DueDate` hoặc `AssignedToUserId` thay đổi → xoá log để có thể re-notify với context mới. Logic này nằm trong `TaskService.UpdateAsync`.

---

## 3. Backend Architecture

### 3.1 Layer mapping (Clean Architecture)

```
CRM.Core
├── Entities/Notification.cs
├── Entities/NotificationRolePreference.cs
├── Entities/TaskNotificationLog.cs
├── Enums/NotificationType.cs
├── Enums/NotificationSeverity.cs
└── Interfaces/Repositories/
    ├── INotificationRepository.cs
    ├── INotificationRolePreferenceRepository.cs
    └── ITaskNotificationLogRepository.cs

CRM.Application
├── DTOs/Notification/
│   ├── NotificationDto.cs
│   ├── NotificationListResponseDto.cs
│   ├── NotificationRolePreferenceDto.cs
│   └── UpdateRolePreferencesRequest.cs
├── Interfaces/
│   ├── INotificationService.cs            -- list, mark read, count cho current user
│   ├── INotificationPreferenceService.cs  -- admin chỉnh per-role preferences
│   ├── INotificationDispatcher.cs         -- entry point gọi từ domain services
│   ├── ITaskReminderJob.cs                -- logic scan due/overdue (testable, không tự schedule)
│   ├── IEmailSender.cs                    -- abstraction
│   └── IRealtimeNotifier.cs               -- abstraction cho SignalR (Application không reference SignalR)
├── Services/
│   ├── NotificationService.cs
│   ├── NotificationPreferenceService.cs
│   ├── NotificationDispatcher.cs
│   └── TaskReminderJob.cs                 -- scan + dispatch, gọi từ HostedService
└── Templates/Email/                       -- HTML templates (chuỗi nội suy {{Property}})

CRM.Infrastructure
├── Repositories/
│   ├── NotificationRepository.cs
│   ├── NotificationRolePreferenceRepository.cs
│   └── TaskNotificationLogRepository.cs
├── Services/Email/
│   ├── SmtpEmailSender.cs                 -- impl IEmailSender (MailKit)
│   ├── NoOpEmailSender.cs                 -- fallback khi config rỗng (dev / khi chưa cấp SMTP)
│   └── EmailOptions.cs
└── Services/Realtime/
    └── SignalRNotifier.cs                 -- impl IRealtimeNotifier, gửi qua IHubContext<NotificationHub>

CRM.API
├── Hubs/NotificationHub.cs                -- SignalR hub
├── Controllers/
│   ├── NotificationsController.cs         -- user: list/mark/count
│   └── NotificationPreferencesController.cs -- admin: per-role config
├── BackgroundJobs/
│   ├── NotificationCleanupHostedService.cs -- chạy mỗi 6h
│   └── TaskReminderHostedService.cs        -- chạy mỗi 30 phút, gọi ITaskReminderJob
└── Program.cs                              -- DI + map hub + register hosted services
```

> **Note kiến trúc**: Đặt `*HostedService` ở `CRM.API` (không phải Infrastructure) vì `IHostedService` được .NET host pickup tự động khi DI ở Program.cs — đặt cùng layer wiring là tự nhiên nhất. Logic thật ở `ITaskReminderJob` (Application layer) → testable, dễ migrate sang Hangfire/Quartz sau.

### 3.2 Flow tạo notification

Khi domain service (vd `TaskService.AssignAsync`) cần thông báo:

```csharp
// Trong TaskService:
await _notificationDispatcher.DispatchAsync(new NotificationEvent
{
    Type            = NotificationType.TaskAssigned,
    RecipientUserId = task.AssignedToUserId.Value,
    Title           = "Bạn vừa được giao một công việc mới",
    Message         = $"{task.Title} - hạn {task.DueDate:dd/MM/yyyy}",
    Link            = $"/tasks/{task.Id}",
    EntityType      = "Task",
    EntityId        = task.Id,
    Severity        = NotificationSeverity.Info,
    EmailTemplate   = "TaskAssigned",
    EmailModel      = new { TaskTitle = task.Title, AssignerName = currentUser.FullName, ... }
});
```

`NotificationDispatcher` chịu trách nhiệm:

1. Insert row `Notifications` (luôn luôn nếu user bật InApp).
2. Push real-time qua `IRealtimeNotifier.NotifyUserAsync(userId, dto)`.
3. Nếu user bật Email cho event này → enqueue gửi email (đồng bộ trong giai đoạn 1, async qua queue ở giai đoạn sau nếu cần).

**Resilient**: Lỗi email **không** rollback notification DB. Wrap email send trong `try/catch`, log warning, set flag `EmailSentAt` riêng nếu cần retry sau.

### 3.3 SignalR Hub

```csharp
// CRM.API/Hubs/NotificationHub.cs
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }
}
```

`SignalRNotifier.NotifyUserAsync(userId, dto)` → `_hub.Clients.Group($"user-{userId}").SendAsync("notification", dto)`.

**Auth qua JWT**: SignalR không hỗ trợ header `Authorization` qua WebSocket — phải truyền `access_token` qua query string. Cần `JwtBearerOptions.Events.OnMessageReceived` để lấy token từ `?access_token=` khi `path` bắt đầu bằng `/hubs/notifications`.

```csharp
// Program.cs
builder.Services.AddSignalR();
// ...
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var accessToken = ctx.Request.Query["access_token"];
            var path = ctx.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                ctx.Token = accessToken!;
            }
            return Task.CompletedTask;
        }
    };
    // ... existing TokenValidationParameters ...
});
// ...
app.MapHub<NotificationHub>("/hubs/notifications");
```

### 3.4 Email sender — MailKit

Dùng `MailKit` (NuGet `MailKit`) thay vì `System.Net.Mail.SmtpClient` (MS đã đánh dấu obsolete).

```
EmailOptions:
  Host           : string
  Port           : int (587)
  UseStartTls    : bool (true)
  Username       : string
  Password       : string
  FromAddress    : string
  FromName       : string
  ReplyTo        : string?
```

`SmtpEmailSender.SendAsync(to, subject, htmlBody, plainBody?)` — connect → authenticate → send → disconnect mỗi lần (acceptable cho volume thấp). Khi scale: pool connection.

### 3.5 Email templates

Giai đoạn 1: **chuỗi nội suy** đơn giản, lưu trong file `.html` ở `CRM.Application/Templates/Email/{TemplateName}.html`, copy ra output.

```csharp
public string Render(string templateName, object model)
{
    var path = Path.Combine(AppContext.BaseDirectory, "Templates", "Email", $"{templateName}.html");
    var html = File.ReadAllText(path);
    // Replace {{Property}} bằng giá trị từ model qua reflection
    return SimpleTemplate.Render(html, model);
}
```

Giai đoạn 2: chuyển sang [`Scriban`](https://github.com/scriban/scriban) hoặc Razor để có loop / condition.

### 3.6 Background jobs

**`NotificationCleanupHostedService`** — `PeriodicTimer(6h)`:
- Xoá notification `IsRead=true` AND `CreatedAt < now - 30 days`.
- Xoá notification `IsRead=false` AND `CreatedAt < now - 90 days` (chưa đọc nhưng quá cũ — tránh phình DB).
- Xoá row `TaskNotificationLog` cho task đã `Completed` hoặc đã xoá.

**`TaskReminderHostedService`** — `PeriodicTimer(30 phút)` → gọi `ITaskReminderJob.RunAsync()`:

```csharp
// TaskReminderJob.cs
public async Task RunAsync(CancellationToken ct)
{
    var now = DateTime.UtcNow;
    var horizon = now.AddHours(24);

    // Due-soon: DueDate trong 24h tới, status chưa Completed, chưa có log TaskDueSoon
    var dueSoon = await _taskRepo.QueryDueSoonNotNotifiedAsync(now, horizon, ct);
    foreach (var task in dueSoon)
    {
        await _dispatcher.DispatchAsync(BuildDueSoonEvent(task), ct);
        await _logRepo.AddAsync(task.Id, NotificationType.TaskDueSoon, ct);
    }

    // Overdue: DueDate < now, status chưa Completed, chưa có log TaskOverdue
    var overdue = await _taskRepo.QueryOverdueNotNotifiedAsync(now, ct);
    foreach (var task in overdue)
    {
        await _dispatcher.DispatchAsync(BuildOverdueEvent(task), ct);
        await _logRepo.AddAsync(task.Id, NotificationType.TaskOverdue, ct);
    }

    await _uow.SaveChangesAsync(ct);
}
```

> **Lưu ý scale**: Khi backend deploy > 1 instance (chưa phải bây giờ), cần đảm bảo job không chạy song song. Lúc đó sẽ:
> - Option 1: Đặt 1 env var `BACKGROUND_JOBS_ENABLED=true` cho duy nhất 1 instance.
> - Option 2: PostgreSQL advisory lock (`pg_try_advisory_lock`) trước khi chạy → instance khác skip.
> - Option 3: Migrate sang Hangfire (có distributed lock built-in).

### 3.7 API endpoints

**User-facing (mọi role có quyền):**
```
GET    /api/notifications              ?page=1&pageSize=20&unreadOnly=true
                                       → list notification của current user
GET    /api/notifications/unread-count → { count: number }
POST   /api/notifications/{id}/read    → mark 1 cái đã đọc
POST   /api/notifications/read-all     → mark tất cả đã đọc
DELETE /api/notifications/{id}         → xoá (chỉ xoá khỏi inbox của mình)
```

**Admin-only (`[Authorize(Roles = "Admin")]`):**
```
GET    /api/admin/notification-preferences          → list config tất cả role × type
PUT    /api/admin/notification-preferences          → bulk update
                                                       body: [{ roleId, type, inApp, email }, ...]
GET    /api/admin/notification-preferences/{roleId} → list config của 1 role
```

**Tất cả** endpoint đều `[Authorize]`. Trong `NotificationsController`:
- `RecipientUserId` luôn = current user (đọc từ JWT claim) — KHÔNG nhận từ query/body.
- Mark/delete: kiểm tra notification thuộc về current user → throw 404 nếu không.

---

## 4. Frontend (Angular 17)

### 4.1 Cấu trúc

```
src/app/
├── core/
│   ├── models/notification.model.ts
│   └── services/
│       ├── notification.service.ts          -- HTTP CRUD + state (BehaviorSubject)
│       └── notification-realtime.service.ts -- SignalR client wrapper
└── features/notifications/
    ├── notifications.module.ts
    ├── notifications-routing.module.ts
    ├── components/
    │   ├── notification-bell/               -- bell icon + badge ở header
    │   ├── notification-dropdown/           -- dropdown 10 cái mới nhất
    │   └── notification-list/               -- full page /notifications
    └── pages/
        ├── notification-list-page/          -- /notifications (mọi user)
        └── role-preferences-admin-page/     -- /admin/notification-preferences (chỉ Admin)
```

> **Sidebar admin entry**: Thêm mục "Cấu hình thông báo" vào sidebar khi user có role Admin (logic guard giống các trang admin hiện tại).

### 4.2 SignalR client

Package: `@microsoft/signalr`.

```typescript
// notification-realtime.service.ts
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class NotificationRealtimeService {
  private hub: HubConnection | null = null;
  private notificationSubject = new Subject<NotificationDto>();
  notification$ = this.notificationSubject.asObservable();

  async connect(): Promise<void> {
    const token = this.auth.getAccessToken();
    if (!token) return;

    this.hub = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/notifications`, {
        accessTokenFactory: () => this.auth.getAccessToken() ?? '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.hub.on('notification', (n: NotificationDto) => this.notificationSubject.next(n));
    await this.hub.start();
  }

  async disconnect(): Promise<void> {
    await this.hub?.stop();
    this.hub = null;
  }
}
```

**Khi nào connect/disconnect?**
- `connect()` khi auth thành công (login + app init nếu đã có token).
- `disconnect()` khi logout / token expired.
- AuthService phát event login/logout → NotificationRealtimeService subscribe.

### 4.3 Notification bell ở header

Thay đổi `header.component.html` — thêm bell icon **trước** user-menu:

```html
<div class="header-right">
  <app-notification-bell></app-notification-bell>
  <div class="user-menu" ...>...</div>
</div>
```

`NotificationBellComponent`:
- Subscribe `notificationService.unreadCount$` → hiển thị badge.
- Click → mở dropdown, gọi `/api/notifications?pageSize=10`.
- Click 1 item → mark read + navigate `notification.link`.
- Footer dropdown: link "Xem tất cả" → `/notifications`.

### 4.4 State management

`NotificationService` giữ `BehaviorSubject<{ items: NotificationDto[]; unreadCount: number }>`. Update từ:

- HTTP fetch khi mở dropdown / page.
- SignalR push: prepend item, increment unread count.
- Mark read action: optimistic update → API call → revert nếu lỗi.

Không cần NgRx cho phạm vi này — service-based state đủ dùng.

### 4.5 Toast cho notification real-time

Khi nhận notification mới qua SignalR → ngoài việc cập nhật bell, nếu severity là `success | warning | error`, hiện thêm `toastService.show(...)` (đã có sẵn trong project) để user thấy ngay cả khi không nhìn bell.

> Đối với notification `info` (vd `TaskCompleted` của người khác), chỉ tăng badge, không toast — tránh ồn.

### 4.6 Admin Preferences page

`/admin/notification-preferences` — chỉ Admin truy cập (route guard).

Layout: matrix `Role × NotificationType`, mỗi cell có 2 checkbox (InApp, Email).

```
              | TaskAssigned | TaskDueSoon | TaskOverdue | TaskCompleted | TaskReassigned
SalesManager  | [☑] [☑]      | [☑] [☑]    | [☑] [☑]    | [☑] [☐]      | [☑] [☐]
SalesRep      | [☑] [☑]      | [☑] [☑]    | [☑] [☑]    | [☐] [☐]      | [☑] [☐]
DesignManager | [☑] [☑]      | [☑] [☑]    | [☑] [☑]    | [☑] [☐]      | [☑] [☐]
Designer      | [☑] [☑]      | [☑] [☑]    | [☑] [☑]    | [☐] [☐]      | [☑] [☐]
ContentMgr    | [☑] [☑]      | [☑] [☑]    | [☑] [☑]    | [☑] [☐]      | [☑] [☐]
ContentStaff  | [☑] [☑]      | [☑] [☑]    | [☑] [☑]    | [☐] [☐]      | [☑] [☐]
```

> Các role khác (Production, QC, Delivery, ...) ẩn ở giai đoạn 1 — chỉ show 6 role thuộc Sale/Design/Content.

Action button: "Khôi phục mặc định" → reset về `appsettings.json` defaults.

Reactive form, gửi PUT `/api/admin/notification-preferences`. Backend validate caller có role `Admin`.

---

## 5. Cấu hình `appsettings.json`

Thêm section mới:

```json
{
  "Notifications": {
    "RetentionReadDays": 30,
    "RetentionUnreadDays": 90,
    "RoleDefaults": {
      "SalesManager": {
        "TaskAssigned":   { "InApp": true, "Email": true  },
        "TaskDueSoon":    { "InApp": true, "Email": true  },
        "TaskOverdue":    { "InApp": true, "Email": true  },
        "TaskCompleted":  { "InApp": true, "Email": false },
        "TaskReassigned": { "InApp": true, "Email": false }
      },
      "SalesRep": {
        "TaskAssigned":   { "InApp": true, "Email": true  },
        "TaskDueSoon":    { "InApp": true, "Email": true  },
        "TaskOverdue":    { "InApp": true, "Email": true  },
        "TaskCompleted":  { "InApp": false, "Email": false },
        "TaskReassigned": { "InApp": true, "Email": false }
      },
      "DesignManager": { /* giống SalesManager */ },
      "Designer":      { /* giống SalesRep */ },
      "ContentManager":{ /* giống SalesManager */ },
      "ContentStaff":  { /* giống SalesRep */ },
      "Admin": {
        "_FALLBACK_ALL": { "InApp": true, "Email": false }
      }
    },
    "JobIntervals": {
      "CleanupHours": 6,
      "TaskReminderMinutes": 30
    }
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseStartTls": true,
    "Username": "",
    "Password": "",
    "FromAddress": "",
    "FromName": "CRM Đồng Phục Bốn Mùa",
    "ReplyTo": ""
  }
}
```

> **Khi `Email.Host` hoặc `Username` rỗng** → DI register `NoOpEmailSender` (log warning + skip), giống pattern GHTK hiện tại. Cho phép dev chạy mà không cần cấu hình SMTP. Khi bạn cung cấp credentials → điền vào `appsettings.Production.json` (hoặc env var `Email__Username`, `Email__Password`) để override.

> **Google Workspace cụ thể**:
> - `Host: smtp.gmail.com`
> - `Port: 587`
> - `UseStartTls: true`
> - `Username`: full email account (vd `noreply@dongphucbonmua.com`)
> - `Password`: App Password 16 ký tự (KHÔNG phải password thường — yêu cầu account đã bật 2FA)
> - `FromAddress`: trùng `Username` (nếu khác → Google reject vì policy)

---

## 6. Migration

### 6.1 EF Core migration

```
dotnet ef migrations add AddNotifications --project CRM.Infrastructure --startup-project CRM.API
dotnet ef database update --project CRM.Infrastructure --startup-project CRM.API
```

Tạo bảng `Notifications`, `NotificationRolePreferences`, `TaskNotificationLog`, kèm index như mục 2.

### 6.2 Seed dữ liệu

- `Notifications`: không seed — sinh ra khi có sự kiện.
- `TaskNotificationLog`: không seed — sinh ra từ background job.
- `NotificationRolePreferences`: **không seed**. Logic resolver fallback về `appsettings.json:Notifications:RoleDefaults` khi DB không có row. Admin chỉ insert khi muốn override default.

> Lý do: nếu seed sẵn 6 role × 5 type = 30 row, mọi thay đổi default trong code đều phải migration thêm — phức tạp. Fallback config-first đơn giản hơn.

---

## 7. Roadmap triển khai

### Giai đoạn 1A — Foundation + In-app + SignalR cho Task (2 ngày)

Backend:
- [ ] Tạo entities: `Notification`, `NotificationRolePreference`, `TaskNotificationLog`.
- [ ] Enums: `NotificationType`, `NotificationSeverity`.
- [ ] EF Core configurations + migration `AddNotifications`.
- [ ] Repositories: `INotificationRepository`, `INotificationRolePreferenceRepository`, `ITaskNotificationLogRepository` + impl.
- [ ] DTOs + `INotificationService` + `NotificationService` (list, count, mark read).
- [ ] `INotificationPreferenceService` + impl (resolve role config với DB → fallback config).
- [ ] `INotificationDispatcher` + `NotificationDispatcher` (in-app + realtime, **email là no-op tạm thời** — `NoOpEmailSender`).
- [ ] `INotificationHub` + setup SignalR + JWT query-string auth trong `Program.cs`.
- [ ] `IRealtimeNotifier` + `SignalRNotifier`.
- [ ] `NotificationsController` (user) + `NotificationPreferencesController` (admin).
- [ ] Tích hợp `NotificationDispatcher` vào `TaskService.CreateAsync`, `UpdateAsync`, `AssignAsync` cho 5 event type.
- [ ] Khi `TaskItem.AssignedToUserId` hoặc `DueDate` thay đổi → xoá `TaskNotificationLog` row tương ứng.

Frontend:
- [ ] `notification.model.ts`.
- [ ] `notification.service.ts` (HTTP CRUD + state).
- [ ] `notification-realtime.service.ts` (SignalR client, install `@microsoft/signalr`).
- [ ] Hook connect/disconnect vào `AuthService` (login/logout).
- [ ] `NotificationBellComponent` + tích hợp vào `header.component.html`.
- [ ] `NotificationDropdownComponent`.
- [ ] `NotificationListPageComponent` route `/notifications`.
- [ ] Toast khi `severity ≥ warning`.

Verify end-to-end:
- [ ] Login 2 tab cùng user → assign task tab A → bell tab B tăng badge < 2s.
- [ ] Login 2 user khác nhau → user A tạo task assign cho user B → user B nhận notification, A không.

### Giai đoạn 1B — Background jobs cho Task reminder (0.5 ngày)
- [ ] `ITaskReminderJob` + `TaskReminderJob` (logic scan due-soon/overdue).
- [ ] `TaskReminderHostedService` (`PeriodicTimer(30')` → gọi job).
- [ ] `NotificationCleanupHostedService` (`PeriodicTimer(6h)`).
- [ ] Manual test: tạo task `DueDate = now + 23h` → đợi job chạy → verify notification + log row.

### Giai đoạn 1C — Email (sau khi có SMTP credentials, 0.5 ngày)
- [ ] Thêm `MailKit` NuGet package.
- [ ] `IEmailSender` + `SmtpEmailSender` + `EmailOptions`.
- [ ] DI condition: nếu `EmailOptions.Username` rỗng → register `NoOpEmailSender` thay vì `SmtpEmailSender`.
- [ ] Templates HTML: `TaskAssigned.html`, `TaskDueSoon.html`, `TaskOverdue.html`, `TaskReassigned.html`.
- [ ] `SimpleTemplate.Render` helper (replace `{{Property}}`).
- [ ] Mở rộng `NotificationDispatcher`: đọc preference, nếu `Email=true` → gọi `IEmailSender.SendAsync`.
- [ ] Test: gửi email thật từ Google Workspace SMTP → verify HTML render đúng.

### Giai đoạn 1D — Admin preferences UI (0.5 ngày)
- [ ] Frontend page `/admin/notification-preferences` (matrix Role × Type).
- [ ] Route guard chỉ Admin.
- [ ] Sidebar entry (chỉ hiện khi Admin).
- [ ] Action "Khôi phục mặc định".

### Giai đoạn 2 — Phủ Deal (sau khi 1A-1D ổn định)
- [ ] Thêm enum `DealAssigned`, `DealStageChanged`, `DealClosed`.
- [ ] Tích hợp dispatcher vào `DealService.AssignAsync`, `UpdateStageAsync`, `CloseAsync`.
- [ ] Templates email tương ứng.
- [ ] Update admin preferences UI để hiển thị các type mới.

### Giai đoạn 3 — Phủ Order + Production + GHTK + SePay
- [ ] Order events (`OrderCreated`, `OrderStatusChanged`, `OrderPaymentReceived`, `OrderDeliveryUpdated`).
- [ ] Production events (`ProductionStepBlocked`, `QcRejected`).
- [ ] GHTK webhook handler → dispatch.
- [ ] SePay webhook handler → dispatch.

### Giai đoạn 4 — Mở rộng (sau)
- [ ] Push notification mobile (FCM) — khi có app mobile.
- [ ] Slack / Telegram bot integration (nếu team yêu cầu).
- [ ] Hangfire nếu phải scale > 1 backend instance.
- [ ] OAuth2 thay App Password cho SMTP.

---

## 8. Rủi ro & lưu ý

- **JWT trong query string** (cho SignalR) sẽ bị log ở Nginx access log mặc định. Production nên cấu hình Nginx `log_format` strip `access_token` query param hoặc chuyển sang Cookie auth cho hub.
- **Email vào spam**: bắt buộc cấu hình SPF + DKIM cho domain gửi. Nếu dùng Gmail SMTP từ domain `@dongphucbonmua.com` → 100% vào spam. Phải dùng dịch vụ chuyên dụng + verify domain.
- **N+1 query khi list notification**: query bao gồm join `Users` nếu cần avatar người gửi → eager load hoặc projection thẳng sang DTO.
- **Notification storm**: khi 1 sự kiện tạo nhiều notification (vd. order broadcast cho 20 sale) → batch insert, không loop insert từng cái.
- **Privacy**: Email body không nên chứa thông tin nhạy cảm (giá đơn, thông tin khách hàng đầy đủ) — chỉ tiêu đề + link CRM. Dù SMTP TLS, email forwarding ngoài tầm kiểm soát.
- **Time zone**: Tất cả notification `CreatedAt` lưu UTC, frontend convert sang `Asia/Ho_Chi_Minh` để hiển thị "5 phút trước".
- **Seed test**: Trong dev, có 1 endpoint `POST /api/notifications/test` (dev-only, gated bằng `app.Environment.IsDevelopment()`) để dispatch notification giả lập, giúp dev frontend không cần trigger event thật.

---

## 9. Test plan

- [ ] Unit test `NotificationService.MarkReadAsync` — chỉ user sở hữu được mark.
- [ ] Unit test `NotificationDispatcher` — verify đúng kênh được gọi theo preference.
- [ ] Integration test endpoint `/api/notifications` — pagination, filter unread.
- [ ] Manual: mở 2 tab cùng user → mark read tab A → tab B reflect (cần broadcast `notification-read` qua hub).
- [ ] Manual: gửi email thử qua dev SMTP (Mailtrap.io / Mailpit local) → verify HTML render đúng.
- [ ] Manual: tắt SignalR (DevTools Network → block ws) → verify HTTP polling fallback hoạt động (SignalR tự fallback long-polling).
- [ ] Manual: assign 50 task liên tiếp → verify bell counter chính xác, không miss event.

---

## 10. Tóm tắt quyết định đã chốt

| Câu hỏi | Quyết định |
|---|---|
| Preferences | **Admin-only**, per-role (không cho user tự chỉnh) |
| Email overdue | **Real-time** |
| Retention | **30 ngày** (read) / **90 ngày** (unread) — `IHostedService` |
| SMTP | **Google Workspace** + App Password (credentials cấp sau) |
| Phạm vi phase 1 | **Task only**, target Sale + Design + Content |
| Notify khách hàng | **Không** (out of scope) |

## 11. Sẵn sàng để bắt đầu

Có thể bắt đầu code giai đoạn 1A ngay (không cần SMTP credentials — phần email tạm thời là `NoOpEmailSender`).

Khi bạn cung cấp SMTP credentials sau, code giai đoạn 1C chỉ là swap `NoOpEmailSender` → `SmtpEmailSender` qua DI condition + thêm templates → không phải sửa logic dispatcher.

**Bạn xác nhận để bắt đầu giai đoạn 1A** (entities, migration, in-app notification, SignalR, bell UI, tích hợp vào TaskService) không?
