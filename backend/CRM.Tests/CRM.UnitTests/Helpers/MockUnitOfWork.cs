using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Repositories;
using Moq;

namespace CRM.UnitTests.Helpers;

/// <summary>
/// Wrapper gom các repository mock cần thiết cho ChatService test.
/// Mặc định mỗi repo trả về null/empty — test case override theo nhu cầu.
/// </summary>
internal class MockUnitOfWork
{
    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);
    public Mock<IConversationRepository> Conversations { get; } = new();
    public Mock<IConversationParticipantRepository> Participants { get; } = new();
    public Mock<IChatMessageRepository> Messages { get; } = new();
    public Mock<IUserRepository> Users { get; } = new();

    public int SaveChangesCallCount { get; private set; }

    public MockUnitOfWork()
    {
        UnitOfWork.SetupGet(u => u.Conversations).Returns(() => Conversations.Object);
        UnitOfWork.SetupGet(u => u.ConversationParticipants).Returns(() => Participants.Object);
        UnitOfWork.SetupGet(u => u.ChatMessages).Returns(() => Messages.Object);
        UnitOfWork.SetupGet(u => u.Users).Returns(() => Users.Object);
        UnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(() => { SaveChangesCallCount++; return 1; });
    }

    public void SetupUser(User user)
    {
        Users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
    }

    public void SetupConversation(Conversation conv, Guid viewerId)
    {
        Conversations
            .Setup(r => r.GetWithParticipantsAsync(conv.Id, viewerId))
            .ReturnsAsync(conv);
        Conversations
            .Setup(r => r.GetByIdAsync(conv.Id))
            .ReturnsAsync(conv);
    }

    public void SetupActiveParticipant(Guid conversationId, Guid userId, ConversationParticipant participant)
    {
        Participants
            .Setup(r => r.GetActiveAsync(conversationId, userId))
            .ReturnsAsync(participant);
    }
}
