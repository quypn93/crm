using CRM.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRM.API.Filters;

/// <summary>
/// Đổi exception nghiệp vụ thành mã HTTP có nghĩa, kèm nguyên văn thông báo tiếng Việt:
///   KeyNotFoundException    → 404
///   InvalidOperationException → 400
///
/// Dự án không có middleware xử lý exception toàn cục, các controller cũ tự try/catch từng action.
/// Nhóm controller tài chính có ~20 action nên gắn filter ở lớp cơ sở thay vì lặp try/catch —
/// thiếu một chỗ là người dùng nhận 500 và mất luôn thông báo lý do.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class MapDomainExceptionsAttribute : Attribute, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (status, message) = context.Exception switch
        {
            KeyNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            InvalidOperationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            _ => (0, string.Empty)
        };

        if (status == 0) return;   // để lỗi hệ thống thật vẫn nổi lên log dưới dạng 500

        context.Result = new ObjectResult(ApiResponse.Fail(message)) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
