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


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // All entity configurations are in the Configuration/ folder
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

