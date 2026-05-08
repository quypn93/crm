using System.Security.Claims;
using CRM.API.Hubs;
using CRM.Application.DTOs.Chat;
using CRM.Application.DTOs.Common;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<List<ConversationDto>>>> GetMyConversations()
    {
        var userId = GetCurrentUserId();
        var data = await _chatService.GetMyConversationsAsync(userId);
        return Ok(ApiResponse<List<ConversationDto>>.Ok(data));
    }

    [HttpGet("conversations/{id}")]
    public async Task<ActionResult<ApiResponse<ConversationDto>>> GetConversation(Guid id)
    {
        var userId = GetCurrentUserId();
        var data = await _chatService.GetConversationAsync(id, userId);
        if (data == null)
        {
            return NotFound(ApiResponse<ConversationDto>.Fail("Không tìm thấy cuộc trò chuyện."));
        }
        return Ok(ApiResponse<ConversationDto>.Ok(data));
    }

    [HttpPost("conversations/direct")]
    public async Task<ActionResult<ApiResponse<ConversationDto>>> CreateDirect([FromBody] CreateDirectConversationDto dto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var data = await _chatService.CreateOrGetDirectAsync(userId, dto);
            return Ok(ApiResponse<ConversationDto>.Ok(data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ConversationDto>.Fail(ex.Message));
        }
    }

    [HttpPost("conversations/group")]
    public async Task<ActionResult<ApiResponse<ConversationDto>>> CreateGroup([FromBody] CreateGroupConversationDto dto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var data = await _chatService.CreateGroupAsync(userId, dto);
            return Ok(ApiResponse<ConversationDto>.Ok(data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ConversationDto>.Fail(ex.Message));
        }
    }

    [HttpPut("conversations/{id}/name")]
    public async Task<ActionResult<ApiResponse<ConversationDto>>> Rename(Guid id, [FromBody] RenameGroupDto dto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var data = await _chatService.RenameGroupAsync(id, userId, dto);
            return Ok(ApiResponse<ConversationDto>.Ok(data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ConversationDto>.Fail(ex.Message));
        }
    }

    [HttpPost("conversations/{id}/participants")]
    public async Task<ActionResult<ApiResponse<ConversationDto>>> AddParticipants(Guid id, [FromBody] AddParticipantsDto dto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var data = await _chatService.AddParticipantsAsync(id, userId, dto);
            return Ok(ApiResponse<ConversationDto>.Ok(data));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ConversationDto>.Fail(ex.Message));
        }
    }

    [HttpPost("conversations/{id}/leave")]
    public async Task<ActionResult<ApiResponse>> Leave(Guid id)
    {
        var userId = GetCurrentUserId();
        var ok = await _chatService.LeaveConversationAsync(id, userId);
        if (!ok)
        {
            return NotFound(ApiResponse.Fail("Không tìm thấy cuộc trò chuyện."));
        }
        return Ok(ApiResponse.Ok("Đã rời cuộc trò chuyện."));
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ChatMessageDto>>>> GetMessages(
        Guid id,
        [FromQuery] MessageListFilterDto filter)
    {
        var userId = GetCurrentUserId();
        try
        {
            var data = await _chatService.GetMessagesAsync(id, userId, filter);
            return Ok(ApiResponse<PaginatedResult<ChatMessageDto>>.Ok(data));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<PaginatedResult<ChatMessageDto>>.Fail(ex.Message));
        }
    }

    [HttpPost("conversations/{id}/messages")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage(Guid id, [FromBody] SendMessageDto dto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var data = await _chatService.SendMessageAsync(id, userId, dto);
            return Ok(ApiResponse<ChatMessageDto>.Ok(data));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<ChatMessageDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ChatMessageDto>.Fail(ex.Message));
        }
    }

    [HttpPost("conversations/{id}/read")]
    public async Task<ActionResult<ApiResponse<int>>> MarkRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var totalUnread = await _chatService.MarkReadAsync(id, userId);
        return Ok(ApiResponse<int>.Ok(totalUnread));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var total = await _chatService.GetTotalUnreadAsync(userId);
        return Ok(ApiResponse<int>.Ok(total));
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<ChatUserDto>>>> GetChatUsers()
    {
        var userId = GetCurrentUserId();
        var users = await _chatService.GetChatUsersAsync(userId);
        // Overlay presence từ ChatHub static tracker — service layer không có quyền truy cập SignalR.
        var online = ChatHub.GetOnlineUserIds().ToHashSet();
        foreach (var u in users)
        {
            u.IsOnline = online.Contains(u.Id);
        }
        return Ok(ApiResponse<List<ChatUserDto>>.Ok(users));
    }

    [HttpGet("online-users")]
    public ActionResult<ApiResponse<List<Guid>>> GetOnlineUserIds()
    {
        return Ok(ApiResponse<List<Guid>>.Ok(ChatHub.GetOnlineUserIds().ToList()));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
