namespace CRM.Core.Enums;

public enum DesignStatus
{
    Assigned = 0,     // Sale đã tạo, chờ designer làm
    InProgress = 1,   // Designer đang làm
    Completed = 2,    // Designer đã upload ảnh hoàn thành
    Cancelled = 3
}
