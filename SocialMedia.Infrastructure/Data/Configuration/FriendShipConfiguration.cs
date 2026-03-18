using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Infrastructure.Data.Configuration;

public class FriendShipConfiguration : IEntityTypeConfiguration<FriendShip>
{
    public void Configure(EntityTypeBuilder<FriendShip> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.SenderId)
            .IsRequired();

        builder.Property(f => f.ReceiverId)
            .IsRequired();

        // Relationships
        builder.HasOne(f => f.Sender)
            .WithMany(u => u.SentFriendRequests)
            .HasForeignKey(f => f.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(f => f.Receiver)
            .WithMany(u => u.ReceivedFriendRequests)
            .HasForeignKey(f => f.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction);

        // Unique constraint: prevent duplicate friendships
        builder.HasIndex(f => new { f.SenderId, f.ReceiverId })
            .IsUnique();

        // Indexes
        builder.HasIndex(f => f.SenderId);
        builder.HasIndex(f => f.ReceiverId);
    }
}
