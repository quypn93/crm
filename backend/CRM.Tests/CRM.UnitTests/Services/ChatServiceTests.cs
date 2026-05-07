using AutoMapper;
using CRM.Application.DTOs.Chat;
using CRM.Application.Interfaces;
using CRM.Application.Mappings;
using CRM.Application.Services;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.UnitTests.Helpers;
using Moq;

namespace CRM.UnitTests.Services;

public class ChatServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IChatRealtimeNotifier> _realtime;
    private readonly MockUnitOfWork _uow;
    private readonly ChatService _service;

    public ChatServiceTests()
    {
        // Dùng MappingProfile thật để tránh divergence giữa test và production AutoMapper config.
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        _mapper = cfg.CreateMapper();

        _realtime = new Mock<IChatRealtimeNotifier>();
        _uow = new MockUnitOfWork();
        _service = new ChatService(_uow.UnitOfWork.Object, _mapper, _realtime.Object);
    }

    // ---------------------------------------------------------------------
    // CreateOrGetDirectAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CreateOrGetDirect_SelfChat_Throws()
    {
        var userId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateOrGetDirectAsync(userId, new CreateDirectConversationDto { OtherUserId = userId }));

        Assert.Contains("chính mình", ex.Message);
    }

    [Fact]
    public async Task CreateOrGetDirect_OtherUserNotFound_Throws()
    {
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        _uow.Users.Setup(r => r.GetByIdAsync(other)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateOrGetDirectAsync(me, new CreateDirectConversationDto { OtherUserId = other }));
    }

    [Fact]
    public async Task CreateOrGetDirect_InactiveUser_Throws()
    {
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        _uow.SetupUser(ChatTestData.MakeUser(other, isActive: false));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateOrGetDirectAsync(me, new CreateDirectConversationDto { OtherUserId = other }));
    }

    [Fact]
    public async Task CreateOrGetDirect_WhenExisting_ReturnsExistingWithoutSaveOrBroadcast()
    {
        var meId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var existing = ChatTestData.MakeDirectConversation(meId, otherId, out _, out var other);

        _uow.SetupUser(other);
        _uow.Conversations.Setup(r => r.FindDirectAsync(meId, otherId)).ReturnsAsync(existing);

        var result = await _service.CreateOrGetDirectAsync(meId,
            new CreateDirectConversationDto { OtherUserId = otherId });

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(0, _uow.SaveChangesCallCount);
        _uow.Conversations.Verify(r => r.AddAsync(It.IsAny<Conversation>()), Times.Never);
        _realtime.Verify(r => r.NotifyConversationUpsertAsync(
            It.IsAny<IEnumerable<Guid>>(), It.IsAny<ConversationDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateOrGetDirect_WhenNew_PersistsAndBroadcastsToBoth()
    {
        var meId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        _uow.SetupUser(ChatTestData.MakeUser(otherId));
        _uow.Conversations.Setup(r => r.FindDirectAsync(meId, otherId)).ReturnsAsync((Conversation?)null);

        Conversation? captured = null;
        _uow.Conversations.Setup(r => r.AddAsync(It.IsAny<Conversation>()))
            .Callback<Conversation>(c => captured = c)
            .ReturnsAsync((Conversation c) => c);

        // Sau save, BuildDtoAsync sẽ gọi GetWithParticipantsAsync với cả 2 userId.
        _uow.Conversations.Setup(r => r.GetWithParticipantsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(() => captured);

        var result = await _service.CreateOrGetDirectAsync(meId,
            new CreateDirectConversationDto { OtherUserId = otherId });

        Assert.NotNull(captured);
        Assert.Equal(ConversationType.Direct, captured!.Type);
        Assert.Equal(2, captured.Participants.Count);
        Assert.Contains(captured.Participants, p => p.UserId == meId && p.IsActive);
        Assert.Contains(captured.Participants, p => p.UserId == otherId && p.IsActive);
        Assert.True(_uow.SaveChangesCallCount >= 1);

        // Realtime upsert được gọi 2 lần (1 lần / user).
        _realtime.Verify(r => r.NotifyConversationUpsertAsync(
            It.IsAny<IEnumerable<Guid>>(), It.IsAny<ConversationDto>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    // ---------------------------------------------------------------------
    // CreateGroupAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CreateGroup_EmptyName_Throws()
    {
        var me = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateGroupAsync(me, new CreateGroupConversationDto
            {
                Name = "   ",
                MemberUserIds = new List<Guid> { Guid.NewGuid() }
            }));
    }

    [Fact]
    public async Task CreateGroup_NoMembers_Throws()
    {
        var me = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateGroupAsync(me, new CreateGroupConversationDto
            {
                Name = "Team",
                MemberUserIds = new List<Guid>() // empty
            }));
    }

    [Fact]
    public async Task CreateGroup_OnlyCreatorInMemberList_Throws()
    {
        // Nếu MemberUserIds chỉ chứa creator, sau khi loại creator danh sách rỗng → throw.
        var me = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateGroupAsync(me, new CreateGroupConversationDto
            {
                Name = "Team",
                MemberUserIds = new List<Guid> { me }
            }));
    }

    [Fact]
    public async Task CreateGroup_Success_CreatorIsAdminAndAllUsersBroadcast()
    {
        var me = Guid.NewGuid();
        var memberA = Guid.NewGuid();
        var memberB = Guid.NewGuid();
        _uow.SetupUser(ChatTestData.MakeUser(memberA));
        _uow.SetupUser(ChatTestData.MakeUser(memberB));

        Conversation? captured = null;
        _uow.Conversations.Setup(r => r.AddAsync(It.IsAny<Conversation>()))
            .Callback<Conversation>(c => captured = c)
            .ReturnsAsync((Conversation c) => c);
        _uow.Conversations.Setup(r => r.GetWithParticipantsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(() => captured);

        var result = await _service.CreateGroupAsync(me, new CreateGroupConversationDto
        {
            Name = "  Team Chat  ",
            MemberUserIds = new List<Guid> { memberA, memberB, memberA } // có duplicate
        });

        Assert.NotNull(captured);
        Assert.Equal(ConversationType.Group, captured!.Type);
        Assert.Equal("Team Chat", captured.Name);
        Assert.Equal(3, captured.Participants.Count); // me + memberA + memberB (duplicate bị loại)
        var creatorPart = Assert.Single(captured.Participants, p => p.UserId == me);
        Assert.True(creatorPart.IsAdmin);
        Assert.True(creatorPart.IsActive);

        // 1 broadcast / member
        _realtime.Verify(r => r.NotifyConversationUpsertAsync(
            It.IsAny<IEnumerable<Guid>>(), It.IsAny<ConversationDto>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    // ---------------------------------------------------------------------
    // SendMessageAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task SendMessage_EmptyContent_Throws()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SendMessageAsync(convId, me, new SendMessageDto { Content = "   " }));
    }

    [Fact]
    public async Task SendMessage_NotMember_ThrowsUnauthorized()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();
        _uow.Participants.Setup(r => r.GetActiveAsync(convId, me))
            .ReturnsAsync((ConversationParticipant?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.SendMessageAsync(convId, me, new SendMessageDto { Content = "hello" }));
    }

    [Fact]
    public async Task SendMessage_Success_PersistsAndUpdatesConversationAndBroadcasts()
    {
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        var conv = ChatTestData.MakeDirectConversation(me, other, out var meUser, out _);
        var participant = conv.Participants.First(p => p.UserId == me);

        _uow.SetupActiveParticipant(conv.Id, me, participant);
        _uow.Conversations.Setup(r => r.GetByIdAsync(conv.Id)).ReturnsAsync(conv);
        _uow.Conversations.Setup(r => r.GetWithParticipantsAsync(conv.Id, me)).ReturnsAsync(conv);

        ChatMessage? capturedMsg = null;
        _uow.Messages.Setup(r => r.AddAsync(It.IsAny<ChatMessage>()))
            .Callback<ChatMessage>(m => capturedMsg = m)
            .ReturnsAsync((ChatMessage m) => m);
        _uow.Messages.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => capturedMsg);
        _uow.Messages.Setup(r => r.CountUnreadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);
        _uow.Messages.Setup(r => r.CountTotalUnreadAsync(It.IsAny<Guid>())).ReturnsAsync(1);
        _uow.Participants.Setup(r => r.GetActiveUserIdsAsync(conv.Id))
            .ReturnsAsync(new List<Guid> { me, other });
        _uow.Users.Setup(r => r.GetByIdAsync(me)).ReturnsAsync(meUser);

        var dto = await _service.SendMessageAsync(conv.Id, me, new SendMessageDto { Content = "  hello  " });

        Assert.NotNull(capturedMsg);
        Assert.Equal("hello", capturedMsg!.Content); // trim
        Assert.Equal(me, capturedMsg.SenderUserId);
        Assert.Equal(conv.Id, capturedMsg.ConversationId);

        // Conversation summary được cập nhật
        Assert.Equal(capturedMsg.Id, conv.LastMessageId);
        Assert.Equal(capturedMsg.CreatedAt, conv.LastMessageAt);

        // Sender tự động "đọc" message của mình
        Assert.Equal(capturedMsg.CreatedAt, participant.LastReadAt);

        // Realtime: broadcast message tới group + upsert tới mỗi member
        _realtime.Verify(r => r.NotifyMessageAsync(
            It.IsAny<IEnumerable<Guid>>(), It.IsAny<ChatMessageDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _realtime.Verify(r => r.NotifyConversationUpsertAsync(
            It.IsAny<IEnumerable<Guid>>(), It.IsAny<ConversationDto>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        // unread count chỉ push cho người KHÁC sender (1 user).
        _realtime.Verify(r => r.NotifyTotalUnreadAsync(other, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _realtime.Verify(r => r.NotifyTotalUnreadAsync(me, It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        // DTO trả về có sender info đầy đủ
        Assert.Equal("First Last", dto.SenderName);
    }

    [Fact]
    public async Task SendMessage_LongContent_TruncatedTo4000Chars()
    {
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        var conv = ChatTestData.MakeDirectConversation(me, other, out var meUser, out _);
        var participant = conv.Participants.First(p => p.UserId == me);

        _uow.SetupActiveParticipant(conv.Id, me, participant);
        _uow.Conversations.Setup(r => r.GetByIdAsync(conv.Id)).ReturnsAsync(conv);
        _uow.Conversations.Setup(r => r.GetWithParticipantsAsync(conv.Id, me)).ReturnsAsync(conv);

        ChatMessage? captured = null;
        _uow.Messages.Setup(r => r.AddAsync(It.IsAny<ChatMessage>()))
            .Callback<ChatMessage>(m => captured = m)
            .ReturnsAsync((ChatMessage m) => m);
        _uow.Messages.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(() => captured);
        _uow.Messages.Setup(r => r.CountUnreadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);
        _uow.Messages.Setup(r => r.CountTotalUnreadAsync(It.IsAny<Guid>())).ReturnsAsync(0);
        _uow.Participants.Setup(r => r.GetActiveUserIdsAsync(conv.Id))
            .ReturnsAsync(new List<Guid> { me, other });
        _uow.Users.Setup(r => r.GetByIdAsync(me)).ReturnsAsync(meUser);

        var huge = new string('x', 5000);
        await _service.SendMessageAsync(conv.Id, me, new SendMessageDto { Content = huge });

        Assert.NotNull(captured);
        Assert.Equal(4000, captured!.Content.Length);
    }

    // ---------------------------------------------------------------------
    // GetMessagesAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetMessages_NotMember_ThrowsUnauthorized()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();
        _uow.Participants.Setup(r => r.GetActiveAsync(convId, me))
            .ReturnsAsync((ConversationParticipant?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GetMessagesAsync(convId, me, new MessageListFilterDto()));
    }

    [Fact]
    public async Task GetMessages_ClampsPagingBounds()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var participant = new ConversationParticipant { ConversationId = convId, UserId = me, IsActive = true };
        _uow.SetupActiveParticipant(convId, me, participant);

        int? capturedPage = null;
        int? capturedSize = null;
        _uow.Messages.Setup(r => r.GetPagedAsync(convId, It.IsAny<int>(), It.IsAny<int>()))
            .Callback<Guid, int, int>((_, p, s) => { capturedPage = p; capturedSize = s; })
            .ReturnsAsync((Enumerable.Empty<ChatMessage>(), 0));

        // Page âm → ép về 1, pageSize quá lớn → clamp 100.
        await _service.GetMessagesAsync(convId, me, new MessageListFilterDto { Page = -5, PageSize = 9999 });

        Assert.Equal(1, capturedPage);
        Assert.Equal(100, capturedSize);
    }

    // ---------------------------------------------------------------------
    // MarkReadAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task MarkRead_NonParticipant_ReturnsZero()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();
        _uow.Participants.Setup(r => r.GetActiveAsync(convId, me))
            .ReturnsAsync((ConversationParticipant?)null);

        var result = await _service.MarkReadAsync(convId, me);

        Assert.Equal(0, result);
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task MarkRead_AlreadyAtLastMessage_DoesNotPersistButPushesCount()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var lastMsgAt = DateTime.UtcNow.AddMinutes(-5);
        var participant = new ConversationParticipant
        {
            ConversationId = convId,
            UserId = me,
            IsActive = true,
            LastReadAt = lastMsgAt
        };
        var conv = new Conversation { Id = convId, LastMessageAt = lastMsgAt };

        _uow.SetupActiveParticipant(convId, me, participant);
        _uow.Conversations.Setup(r => r.GetByIdAsync(convId)).ReturnsAsync(conv);
        _uow.Messages.Setup(r => r.CountTotalUnreadAsync(me)).ReturnsAsync(7);

        var result = await _service.MarkReadAsync(convId, me);

        // Đã đọc tới mốc rồi → return 0, nhưng vẫn push unread count hiện tại để client đồng bộ.
        Assert.Equal(0, result);
        Assert.Equal(0, _uow.SaveChangesCallCount); // không cập nhật DB
        _realtime.Verify(r => r.NotifyTotalUnreadAsync(me, 7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkRead_NewerMessageExists_UpdatesLastReadAt()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var oldRead = DateTime.UtcNow.AddHours(-1);
        var newLastMsg = DateTime.UtcNow.AddMinutes(-2);
        var participant = new ConversationParticipant
        {
            ConversationId = convId,
            UserId = me,
            IsActive = true,
            LastReadAt = oldRead
        };
        var conv = new Conversation { Id = convId, LastMessageAt = newLastMsg };

        _uow.SetupActiveParticipant(convId, me, participant);
        _uow.Conversations.Setup(r => r.GetByIdAsync(convId)).ReturnsAsync(conv);
        _uow.Messages.Setup(r => r.CountTotalUnreadAsync(me)).ReturnsAsync(0);

        var result = await _service.MarkReadAsync(convId, me);

        Assert.Equal(0, result);
        Assert.Equal(newLastMsg, participant.LastReadAt);
        Assert.True(_uow.SaveChangesCallCount >= 1);
    }

    // ---------------------------------------------------------------------
    // RenameGroupAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task RenameGroup_OnDirectConversation_Throws()
    {
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        var conv = ChatTestData.MakeDirectConversation(me, other, out _, out _);
        _uow.SetupConversation(conv, me);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RenameGroupAsync(conv.Id, me, new RenameGroupDto { Name = "Hi" }));
    }

    [Fact]
    public async Task RenameGroup_EmptyName_Throws()
    {
        var me = Guid.NewGuid();
        var memberA = Guid.NewGuid();
        var conv = ChatTestData.MakeGroupConversation(me, new[] { memberA });
        _uow.SetupConversation(conv, me);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RenameGroupAsync(conv.Id, me, new RenameGroupDto { Name = "  " }));
    }

    [Fact]
    public async Task RenameGroup_Success_UpdatesAndBroadcasts()
    {
        var me = Guid.NewGuid();
        var memberA = Guid.NewGuid();
        var conv = ChatTestData.MakeGroupConversation(me, new[] { memberA });
        _uow.SetupConversation(conv, me);
        _uow.Messages.Setup(r => r.CountUnreadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        await _service.RenameGroupAsync(conv.Id, me, new RenameGroupDto { Name = "  New Name  " });

        Assert.Equal("New Name", conv.Name);
        // Broadcast tới mỗi active member.
        _realtime.Verify(r => r.NotifyConversationUpsertAsync(
            It.IsAny<IEnumerable<Guid>>(), It.IsAny<ConversationDto>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    // ---------------------------------------------------------------------
    // AddParticipantsAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task AddParticipants_OnDirect_Throws()
    {
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        var conv = ChatTestData.MakeDirectConversation(me, other, out _, out _);
        _uow.SetupConversation(conv, me);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddParticipantsAsync(conv.Id, me, new AddParticipantsDto
            {
                UserIds = new List<Guid> { Guid.NewGuid() }
            }));
    }

    [Fact]
    public async Task AddParticipants_AllAlreadyMembers_NoOp()
    {
        var me = Guid.NewGuid();
        var memberA = Guid.NewGuid();
        var conv = ChatTestData.MakeGroupConversation(me, new[] { memberA });
        _uow.SetupConversation(conv, me);
        _uow.Messages.Setup(r => r.CountUnreadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        await _service.AddParticipantsAsync(conv.Id, me, new AddParticipantsDto
        {
            UserIds = new List<Guid> { memberA } // đã là member rồi
        });

        _uow.Participants.Verify(r => r.AddAsync(It.IsAny<ConversationParticipant>()), Times.Never);
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task AddParticipants_NewMembers_PersistsAndBroadcasts()
    {
        var me = Guid.NewGuid();
        var memberA = Guid.NewGuid();
        var newMember = Guid.NewGuid();
        var conv = ChatTestData.MakeGroupConversation(me, new[] { memberA });
        _uow.SetupConversation(conv, me);
        _uow.SetupUser(ChatTestData.MakeUser(newMember));
        _uow.Messages.Setup(r => r.CountUnreadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);

        var added = new List<ConversationParticipant>();
        _uow.Participants.Setup(r => r.AddAsync(It.IsAny<ConversationParticipant>()))
            .Callback<ConversationParticipant>(p =>
            {
                p.User = ChatTestData.MakeUser(p.UserId);
                conv.Participants.Add(p); // simulate refresh
                added.Add(p);
            })
            .ReturnsAsync((ConversationParticipant p) => p);

        await _service.AddParticipantsAsync(conv.Id, me, new AddParticipantsDto
        {
            UserIds = new List<Guid> { newMember, memberA, newMember } // duplicate + existing đều bị skip
        });

        Assert.Single(added);
        Assert.Equal(newMember, added[0].UserId);
        Assert.True(_uow.SaveChangesCallCount >= 1);
    }

    // ---------------------------------------------------------------------
    // LeaveConversationAsync
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Leave_NonMember_ReturnsFalse()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();
        _uow.Participants.Setup(r => r.GetActiveAsync(convId, me))
            .ReturnsAsync((ConversationParticipant?)null);

        var result = await _service.LeaveConversationAsync(convId, me);

        Assert.False(result);
        Assert.Equal(0, _uow.SaveChangesCallCount);
    }

    [Fact]
    public async Task Leave_Success_DeactivatesAndSetsLeftAt()
    {
        var me = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var participant = new ConversationParticipant
        {
            ConversationId = convId,
            UserId = me,
            IsActive = true
        };
        _uow.SetupActiveParticipant(convId, me, participant);
        _uow.Participants.Setup(r => r.GetActiveUserIdsAsync(convId))
            .ReturnsAsync(new List<Guid>()); // không còn ai → bỏ qua broadcast

        var result = await _service.LeaveConversationAsync(convId, me);

        Assert.True(result);
        Assert.False(participant.IsActive);
        Assert.NotNull(participant.LeftAt);
        Assert.True(_uow.SaveChangesCallCount >= 1);
    }
}
