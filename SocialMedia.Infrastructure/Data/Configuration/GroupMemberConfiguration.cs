using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Data.Configuration;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.HasKey(gm => gm.Id);

        builder.Property(gm => gm.UserId)
            .IsRequired();

        builder.Property(gm => gm.GroupId)
            .IsRequired();

        // Relationships
        builder.HasOne(gm => gm.Group)
            .WithMany(g => g.GroupMembers)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gm => gm.User)
            .WithMany(u => u.GroupMembers)
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint: user can only be a member once per group
        builder.HasIndex(gm => new { gm.GroupId, gm.UserId })
            .IsUnique();
    }
}
