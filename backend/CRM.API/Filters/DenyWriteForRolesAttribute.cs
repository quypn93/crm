using System.Security.Claims;
using CRM.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRM.API.Filters;

/// <summary>
/// Chặn mọi request làm thay đổi dữ liệu (POST/PUT/PATCH/DELETE) đối với các role chỉ-được-xem.
///
/// Dùng cho Kế toán: cần đọc dữ liệu nghiệp vụ (đơn hàng, khách hàng…) để đối chiếu khi nhập
/// chi phí, nhưng không được sửa. Đăng ký TOÀN CỤC ở Program.cs thay vì gắn lên từng controller
/// — phần lớn controller chỉ có [Authorize] cấp class nên mọi user đăng nhập đều ghi được;
/// làm thủ công thì sót một endpoint là hở quyền, và controller thêm mới sau cũng dễ quên.
///
/// Những nhánh mà role read-only VẪN phải ghi được thì khai báo ở <see cref="ExemptPathPrefixes"/>.
///
/// User kiêm nhiều role (VD vừa Admin vừa Kế toán) KHÔNG bị chặn: chỉ chặn khi toàn bộ role
/// của user đều nằm trong danh sách chỉ-được-xem.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class DenyWriteForRolesAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _readOnlyRoles;

    /// <summary>Prefix đường dẫn được miễn trừ — role read-only vẫn ghi được ở các nhánh này.</summary>
    public string[] ExemptPathPrefixes { get; init; } = Array.Empty<string>();

    public DenyWriteForRolesAttribute(params string[] readOnlyRoles)
    {
        _readOnlyRoles = readOnlyRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        var method = request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
            return;

        if (ExemptPathPrefixes.Any(p => request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
            return;

        var userRoles = context.HttpContext.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Chưa đăng nhập (webhook ẩn danh, endpoint [AllowAnonymous]) — để [Authorize] tự xử lý.
        if (userRoles.Count == 0) return;

        var isReadOnlyOnly = userRoles.All(r =>
            _readOnlyRoles.Contains(r, StringComparer.OrdinalIgnoreCase));

        if (!isReadOnlyOnly) return;

        context.Result = new ObjectResult(
            ApiResponse.Fail("Tài khoản của bạn chỉ có quyền xem, không được chỉnh sửa dữ liệu này."))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
