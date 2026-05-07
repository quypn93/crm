using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("ConversationParticipants");

        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Một user chỉ active 1 lần trong 1 conversation (unique khi IsActive = true).
        // PostgreSQL hỗ trợ filtered unique index — đảm bảo không ai bị thêm trùng.
        builder.HasIndex(p => new { p.ConversationId, p.UserId })
            .IsUnique()
            .HasFilter("\"IsActive\" = true")
            .HasDatabaseName("IX_ConversationParticipants_Conversation_User_Active");

        builder.HasIndex(p => p.UserId);
    }
}
