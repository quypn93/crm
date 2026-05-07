using CRM.Core.Entities;
using CRM.Core.Enums;

namespace CRM.UnitTests.Helpers;

/// <summary>
/// Helpers tạo entity giả lập cho test ChatService — giảm boilerplate ở từng test case.
/// </summary>
internal static class ChatTestData
{
    public static User MakeUser(Guid id, string first = "First", string last = "Last", bool isActive = true)
    {
        return new User
        {
            Id = id,
            FirstName = first,
            LastName = last,
            Email = $"{first.ToLower()}.{last.ToLower()}@test.local",
            IsActive = isActive
        };
    }

    public static Conversation MakeDirectConversation(
        Guid creatorId,
        Guid otherId,
        out User creator,
        out User other,
        Guid? id = null)
    {
        creator = MakeUser(creatorId, "Alice", "A");
        other = MakeUser(otherId, "Bob", "B");
        var conv = new Conversation
        {
            Id = id ?? Guid.NewGuid(),
            Type = ConversationType.Direct,
            CreatedByUserId = creatorId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        };
        var localCreator = creator;
        var localOther = other;
        conv.Participants = new List<ConversationParticipant>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conv.Id,
                UserId = creatorId,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
                User = localCreator
            },
            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conv.Id,
                UserId = otherId,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
                User = localOther
            }
        };
        return conv;
    }

    public static Conversation MakeGroupConversation(
        Guid creatorId,
        IEnumerable<Guid> memberIds,
        string name = "Group",
        Guid? id = null)
    {
        var conv = new Conversation
        {
            Id = id ?? Guid.NewGuid(),
            Type = ConversationType.Group,
            Name = name,
            CreatedByUserId = creatorId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        };

        var participants = new List<ConversationParticipant>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conv.Id,
                UserId = creatorId,
                IsAdmin = true,
                IsActive = true,
                User = MakeUser(creatorId, "Creator", "C")
            }
        };
        foreach (var mid in memberIds)
        {
            participants.Add(new ConversationParticipant
            {
                Id = Guid.NewGuid(),
                ConversationId = conv.Id,
                UserId = mid,
                IsActive = true,
                User = MakeUser(mid, "Member", mid.ToString().Substring(0, 4))
            });
        }
        conv.Participants = participants;
        return conv;
    }
}
