using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Domain.Entities;


namespace SocialMedia.Infrastructure.Data;
    /// <summary>
    /// Application database context.
    /// Add your DbSet properties for domain entities here.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
{
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupMessages> GroupMessages => Set<GroupMessages>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<FriendShip> FriendShips => Set<FriendShip>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Notification> Notifications => Set<Notification>();


    // TODO: Add DbSet<> properties for your domain entities.
    // Example:
    // public DbSet<Post> Posts => Set<Post>();
    // public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

        builder.Entity<Comment>()
       .HasOne(c => c.Post)
       .WithMany(p => p.Comments)
       .HasForeignKey(c => c.PostId)
       .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Reaction>()
       .HasOne(r => r.Post)
       .WithMany(p => p.Reactions)
       .HasForeignKey(r => r.PostId)
       .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Post>()
       .HasOne(p => p.Creator)
       .WithMany(u => u.Posts)
       .HasForeignKey(p => p.UserId)
       .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Comment>()
     .HasOne(c => c.User)
     .WithMany(u => u.Comments)
     .HasForeignKey(c => c.UserId)
     .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Reaction>()
    .HasOne(r => r.User)
    .WithMany(u => u.Reactions)
    .HasForeignKey(r => r.UserId)
    .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<FriendShip>()
     .HasOne(f => f.Sender)
     .WithMany(u => u.SentFriendRequests)
     .HasForeignKey(f => f.SenderId)
     .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<FriendShip>()
      .HasOne(f => f.Receiver)
      .WithMany(u => u.ReceivedFriendRequests)
      .HasForeignKey(f => f.ReceiverId)
      .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<GroupMember>()
    .HasOne(gm => gm.Group)
    .WithMany(g => g.GroupMembers)
    .HasForeignKey(gm => gm.GroupId)
    .OnDelete(DeleteBehavior.Cascade);
     builder.Entity<GroupMember>()
    .HasOne(gm => gm.User)
    .WithMany(u => u.GroupMembers)
    .HasForeignKey(gm => gm.UserId)
    .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<GroupMessages>()
       .HasOne(gm => gm.Group)
       .WithMany(g => g.GroupMessages)
       .HasForeignKey(gm => gm.GroupId)
       .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<GroupMessages>()
      .HasOne(gm => gm.Sender)
      .WithMany(u => u.GroupMessages)
      .HasForeignKey(gm => gm.SenderId)
      .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Message>()
        .HasOne(m => m.Sender)
        .WithMany(u => u.MessagesSent)
        .HasForeignKey(m => m.SenderId)
        .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Message>()
       .HasOne(m => m.Receiver)
       .WithMany(u => u.MessagesReceived)
       .HasForeignKey(m => m.ReceiverId)
       .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Notification>()
      .HasOne(n => n.User)
      .WithMany(u => u.Notifications)
      .HasForeignKey(n => n.UserId)
      .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Notification>()
    .HasOne(n => n.Sender)
    .WithMany(u => u.SentNotifications)
    .HasForeignKey(n => n.SenderId)
    .OnDelete(DeleteBehavior.NoAction);


        // TODO: Apply entity configurations here.
        // builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    }

