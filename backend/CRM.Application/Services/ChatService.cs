using AutoMapper;
using CRM.Application.DTOs.Chat;
using CRM.Application.DTOs.Common;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IChatRealtimeNotifier _realtime;

    public ChatService(IUnitOfWork unitOfWork, IMapper mapper, IChatRealtimeNotifier realtime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _realtime = realtime;
    }

    public async Task<List<ConversationDto>> GetMyConversationsAsync(Guid userId)
    {
        var conversations = await _unitOfWork.Conversations.GetForUserAsync(userId);
        var result = new List<ConversationDto>(conversations.Count);

        foreach (var conv in conversations)
        {
            var dto = await BuildDtoAsync(conv, userId);
            result.Add(dto);
        }

        return result;
    }

    public async Task<ConversationDto?> GetConversationAsync(Guid conversationId, Guid userId)
    {
        var conv = await _unitOfWork.Conversations.GetWithParticipantsAsync(conversationId, userId);
        if (conv == null) return null;
        return await BuildDtoAsync(conv, userId);
    }

    public async Task<ConversationDto> CreateOrGetDirectAsync(Guid currentUserId, CreateDirectConversationDto dto)
    {
        if (dto.OtherUserId == currentUserId)
        {
            throw new InvalidOperationException("Không thể tạo cuộc trò chuyện với chính mình.");
        }

        var otherUser = await _unitOfWork.Users.GetByIdAsync(dto.OtherUserId)
            ?? throw new InvalidOperationException("Không tìm thấy người dùng.");
        if (!otherUser.IsActive)
        {
            throw new InvalidOperationException("Người dùng đã bị vô hiệu hoá.");
        }

        var existing = await _unitOfWork.Conversations.FindDirectAsync(currentUserId, dto.OtherUserId);
        if (existing != null)
        {
            return await BuildDtoAsync(existing, currentUserId);
        }

        var conversation = new Conversation
        {
            Type = ConversationType.Direct,
            CreatedByUserId = currentUserId,
            Participants = new List<ConversationParticipant>
            {
                new() { UserId = currentUserId, IsActive = true, JoinedAt = DateTime.UtcNow },
                new() { UserId = dto.OtherUserId, IsActive = true, JoinedAt = DateTime.UtcNow }
            }
        };

        await _unitOfWork.Conversations.AddAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        var fresh = await _unitOfWork.Conversations.GetWithParticipantsAsync(conversation.Id, currentUserId)
            ?? conversation;
        var resultDto = await BuildDtoAsync(fresh, currentUserId);

        // Push conversation upsert tới cả 2 user để UI list cập nhật ngay.
        var memberIds = new[] { currentUserId, dto.OtherUserId };
        foreach (var uid in memberIds)
        {
            var perUserDto = await BuildDtoAsync(fresh, uid);
            await _realtime.NotifyConversationUpsertAsync(new[] { uid }, perUserDto);
        }

        return resultDto;
    }

    public async Task<ConversationDto> CreateGroupAsync(Guid currentUserId, CreateGroupConversationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException("Tên nhóm không được để trống.");
        }

        var memberIds = (dto.MemberUserIds ?? new List<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // Creator luôn là member, không cần thêm vào danh sách input.
        memberIds.Remove(currentUserId);

        if (memberIds.Count == 0)
        {
            throw new InvalidOperationException("Nhóm phải có ít nhất 1 thành viên khác.");
        }

        // Validate tất cả userIds tồn tại + active.
        foreach (var memberId in memberIds)
        {
            var u = await _unitOfWork.Users.GetByIdAsync(memberId);
            if (u == null || !u.IsActive)
            {
                throw new InvalidOperationException($"Người dùng {memberId} không hợp lệ.");
            }
        }

        var conversation = new Conversation
        {
            Type = ConversationType.Group,
            Name = dto.Name.Trim(),
            CreatedByUserId = currentUserId,
            Participants = new List<ConversationParticipant>
            {
                new() { UserId = currentUserId, IsAdmin = true, IsActive = true, JoinedAt = DateTime.UtcNow }
            }
        };

        foreach (var id in memberIds)
        {
            conversation.Participants.Add(new ConversationParticipant
            {
                UserId = id,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Conversations.AddAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        var fresh = await _unitOfWork.Conversations.GetWithParticipantsAsync(conversation.Id, currentUserId)
            ?? conversation;
        var resultDto = await BuildDtoAsync(fresh, currentUserId);

        var allMemberIds = new List<Guid>(memberIds) { currentUserId };
        foreach (var uid in allMemberIds)
        {
            var perUserDto = await BuildDtoAsync(fresh, uid);
            await _realtime.NotifyConversationUpsertAsync(new[] { uid }, perUserDto);
        }

        return resultDto;
    }

    public async Task<ConversationDto> RenameGroupAsync(Guid conversationId, Guid currentUserId, RenameGroupDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException("Tên nhóm không được để trống.");
        }

        var conv = await _unitOfWork.Conversations.GetWithParticipantsAsync(conversationId, currentUserId)
            ?? throw new InvalidOperationException("Không tìm thấy cuộc trò chuyện.");

        if (conv.Type != ConversationType.Group)
        {
            throw new InvalidOperationException("Chỉ có thể đổi tên nhóm.");
        }

        conv.Name = dto.Name.Trim();
        _unitOfWork.Conversations.Update(conv);
        await _unitOfWork.SaveChangesAsync();

        var memberIds = conv.Participants.Where(p => p.IsActive).Select(p => p.UserId).ToList();
        foreach (var uid in memberIds)
        {
            var perUserDto = await BuildDtoAsync(conv, uid);
            await _realtime.NotifyConversationUpsertAsync(new[] { uid }, perUserDto);
        }

        return await BuildDtoAsync(conv, currentUserId);
    }

    public async Task<ConversationDto> AddParticipantsAsync(Guid conversationId, Guid currentUserId, AddParticipantsDto dto)
    {
        var conv = await _unitOfWork.Conversations.GetWithParticipantsAsync(conversationId, currentUserId)
            ?? throw new InvalidOperationException("Không tìm thấy cuộc trò chuyện.");

        if (conv.Type != ConversationType.Group)
        {
            throw new InvalidOperationException("Chỉ có thể thêm thành viên vào nhóm.");
        }

        var existingActiveIds = conv.Participants.Where(p => p.IsActive).Select(p => p.UserId).ToHashSet();
        var toAdd = (dto.UserIds ?? new List<Guid>())
            .Distinct()
            .Where(id => id != Guid.Empty && !existingActiveIds.Contains(id))
            .ToList();

        if (toAdd.Count == 0)
        {
            return await BuildDtoAsync(conv, currentUserId);
        }

        foreach (var id in toAdd)
        {
            var u = await _unitOfWork.Users.GetByIdAsync(id);
            if (u == null || !u.IsActive)
            {
                throw new InvalidOperationException($"Người dùng {id} không hợp lệ.");
            }

            await _unitOfWork.ConversationParticipants.AddAsync(new ConversationParticipant
            {
                ConversationId = conv.Id,
                UserId = id,
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();

        var refreshed = await _unitOfWork.Conversations.GetWithParticipantsAsync(conv.Id, currentUserId) ?? conv;
        var memberIds = refreshed.Participants.Where(p => p.IsActive).Select(p => p.UserId).ToList();
        foreach (var uid in memberIds)
        {
            var perUserDto = await BuildDtoAsync(refreshed, uid);
            await _realtime.NotifyConversationUpsertAsync(new[] { uid }, perUserDto);
        }

        return await BuildDtoAsync(refreshed, currentUserId);
    }

    public async Task<bool> LeaveConversationAsync(Guid conversationId, Guid currentUserId)
    {
        var participant = await _unitOfWork.ConversationParticipants.GetActiveAsync(conversationId, currentUserId);
        if (participant == null) return false;

        participant.IsActive = false;
        participant.LeftAt = DateTime.UtcNow;
        _unitOfWork.ConversationParticipants.Update(participant);
        await _unitOfWork.SaveChangesAsync();

        // Push update tới các thành viên còn lại để họ thấy người này đã rời.
        var remainingIds = await _unitOfWork.ConversationParticipants.GetActiveUserIdsAsync(conversationId);
        if (remainingIds.Count > 0)
        {
            var conv = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
            if (conv != null)
            {
                var refreshed = await _unitOfWork.Conversations.GetWithParticipantsAsync(conversationId, remainingIds[0]);
                if (refreshed != null)
                {
                    foreach (var uid in remainingIds)
                    {
                        var perUserDto = await BuildDtoAsync(refreshed, uid);
                        await _realtime.NotifyConversationUpsertAsync(new[] { uid }, perUserDto);
                    }
                }
            }
        }

        return true;
    }

    public async Task<PaginatedResult<ChatMessageDto>> GetMessagesAsync(Guid conversationId, Guid currentUserId, MessageListFilterDto filter)
    {
        // Verify membership trước khi đọc message.
        var participant = await _unitOfWork.ConversationParticipants.GetActiveAsync(conversationId, currentUserId)
            ?? throw new UnauthorizedAccessException("Bạn không phải thành viên của cuộc trò chuyện này.");

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var (items, total) = await _unitOfWork.ChatMessages.GetPagedAsync(conversationId, page, pageSize);
        var dtos = _mapper.Map<List<ChatMessageDto>>(items);
        return PaginatedResult<ChatMessageDto>.Create(dtos, total, page, pageSize);
    }

    public async Task<ChatMessageDto> SendMessageAsync(Guid conversationId, Guid currentUserId, SendMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new InvalidOperationException("Nội dung tin nhắn không được để trống.");
        }

        var content = dto.Content.Trim();
        if (content.Length > 4000)
        {
            content = content.Substring(0, 4000);
        }

        var participant = await _unitOfWork.ConversationParticipants.GetActiveAsync(conversationId, currentUserId)
            ?? throw new UnauthorizedAccessException("Bạn không phải thành viên của cuộc trò chuyện này.");

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderUserId = currentUserId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ChatMessages.AddAsync(message);

        // Update conversation summary để list sort + preview chính xác.
        var conv = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
        if (conv != null)
        {
            conv.LastMessageId = message.Id;
            conv.LastMessageAt = message.CreatedAt;
            _unitOfWork.Conversations.Update(conv);
        }

        // Sender tự động đọc message của chính mình → cập nhật LastReadAt để unread count = 0.
        participant.LastReadAt = message.CreatedAt;
        _unitOfWork.ConversationParticipants.Update(participant);

        await _unitOfWork.SaveChangesAsync();

        // Re-load với SenderUser navigation để map đầy đủ tên + avatar.
        var fresh = await _unitOfWork.ChatMessages.GetByIdAsync(message.Id) ?? message;
        // GetByIdAsync ở Repository<T> dùng FindAsync nên không có Include — ta lookup user thủ công.
        var sender = await _unitOfWork.Users.GetByIdAsync(currentUserId);
        var dtoOut = _mapper.Map<ChatMessageDto>(fresh);
        if (sender != null)
        {
            dtoOut.SenderName = $"{sender.FirstName} {sender.LastName}";
            dtoOut.SenderAvatarUrl = sender.AvatarUrl;
        }

        var memberIds = await _unitOfWork.ConversationParticipants.GetActiveUserIdsAsync(conversationId);

        // Broadcast tin nhắn tới mọi thành viên active.
        await _realtime.NotifyMessageAsync(memberIds, dtoOut);

        // Cập nhật conversation summary cho từng member (mỗi user có unreadCount khác nhau).
        var convFresh = await _unitOfWork.Conversations.GetWithParticipantsAsync(conversationId, currentUserId);
        if (convFresh != null)
        {
            foreach (var uid in memberIds)
            {
                var perUserDto = await BuildDtoAsync(convFresh, uid);
                await _realtime.NotifyConversationUpsertAsync(new[] { uid }, perUserDto);

                if (uid != currentUserId)
                {
                    var totalUnread = await _unitOfWork.ChatMessages.CountTotalUnreadAsync(uid);
                    await _realtime.NotifyTotalUnreadAsync(uid, totalUnread);
                }
            }
        }

        return dtoOut;
    }

    public async Task<int> MarkReadAsync(Guid conversationId, Guid currentUserId)
    {
        var participant = await _unitOfWork.ConversationParticipants.GetActiveAsync(conversationId, currentUserId);
        if (participant == null) return 0;

        var conv = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
        var newReadAt = conv?.LastMessageAt ?? DateTime.UtcNow;

        if (participant.LastReadAt.HasValue && participant.LastReadAt >= newReadAt)
        {
            // Đã đọc tới mốc này rồi.
            var existingTotal = await _unitOfWork.ChatMessages.CountTotalUnreadAsync(currentUserId);
            await _realtime.NotifyTotalUnreadAsync(currentUserId, existingTotal);
            return 0;
        }

        participant.LastReadAt = newReadAt;
        _unitOfWork.ConversationParticipants.Update(participant);
        await _unitOfWork.SaveChangesAsync();

        var totalUnread = await _unitOfWork.ChatMessages.CountTotalUnreadAsync(currentUserId);
        await _realtime.NotifyTotalUnreadAsync(currentUserId, totalUnread);
        return totalUnread;
    }

    public Task<int> GetTotalUnreadAsync(Guid userId)
    {
        return _unitOfWork.ChatMessages.CountTotalUnreadAsync(userId);
    }

    public async Task<List<ChatUserDto>> GetChatUsersAsync(Guid currentUserId)
    {
        var users = await _unitOfWork.Users.GetAllWithRolesAsync();
        return users
            .Where(u => u.IsActive && u.Id != currentUserId)
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Select(u => new ChatUserDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                PrimaryRole = u.UserRoles?.Select(ur => ur.Role?.Name).FirstOrDefault(name => !string.IsNullOrEmpty(name)),
                IsOnline = false
            })
            .ToList();
    }

    private async Task<ConversationDto> BuildDtoAsync(Conversation conv, Guid currentUserId)
    {
        var dto = new ConversationDto
        {
            Id = conv.Id,
            Type = conv.Type,
            Name = conv.Name,
            CreatedByUserId = conv.CreatedByUserId,
            CreatedAt = conv.CreatedAt,
            LastMessageAt = conv.LastMessageAt
        };

        var activeParticipants = conv.Participants?.Where(p => p.IsActive).ToList()
            ?? await _unitOfWork.ConversationParticipants.GetActiveByConversationAsync(conv.Id);
        dto.Participants = _mapper.Map<List<ConversationParticipantDto>>(activeParticipants);

        // DisplayName: với direct là tên user còn lại; với group thì dùng Name (fallback ghép tên).
        if (conv.Type == ConversationType.Direct)
        {
            var other = activeParticipants.FirstOrDefault(p => p.UserId != currentUserId);
            dto.DisplayName = other?.User != null
                ? $"{other.User.FirstName} {other.User.LastName}"
                : (conv.Name ?? "Cuộc trò chuyện");
        }
        else
        {
            dto.DisplayName = !string.IsNullOrWhiteSpace(conv.Name)
                ? conv.Name!
                : string.Join(", ", activeParticipants
                    .Where(p => p.UserId != currentUserId)
                    .Select(p => p.User != null ? $"{p.User.FirstName}" : "?")
                    .Take(3));
        }

        // Last message preview.
        if (conv.LastMessageId.HasValue)
        {
            var lastMsg = await _unitOfWork.ChatMessages.GetByIdAsync(conv.LastMessageId.Value);
            if (lastMsg != null && !lastMsg.IsDeleted)
            {
                dto.LastMessagePreview = lastMsg.Content.Length > 120
                    ? lastMsg.Content.Substring(0, 120) + "…"
                    : lastMsg.Content;
                dto.LastMessageSenderId = lastMsg.SenderUserId;
            }
        }

        // Unread count cho user hiện tại.
        var participant = activeParticipants.FirstOrDefault(p => p.UserId == currentUserId);
        if (participant != null)
        {
            dto.UnreadCount = await _unitOfWork.ChatMessages.CountUnreadAsync(conv.Id, currentUserId, participant.LastReadAt);
        }

        return dto;
    }
}
