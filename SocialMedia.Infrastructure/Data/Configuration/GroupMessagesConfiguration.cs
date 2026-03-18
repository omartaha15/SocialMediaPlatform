using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Data.Configuration;

public class GroupMessagesConfiguration : IEntityTypeConfiguration<GroupMessages>
{
    public void Configure(EntityTypeBuilder<GroupMessages> builder)
    {
        builder.HasKey(gm => gm.Id);

        builder.Property(gm => gm.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(gm => gm.SenderId)
            .IsRequired();

        builder.Property(gm => gm.GroupId)
            .IsRequired();

        // Relationships
        builder.HasOne(gm => gm.Group)
            .WithMany(g => g.GroupMessages)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gm => gm.Sender)
            .WithMany(u => u.GroupMessages)
            .HasForeignKey(gm => gm.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(gm => new { gm.GroupId, gm.CreatedAt });
        builder.HasIndex(gm => gm.SenderId);
    }
}
