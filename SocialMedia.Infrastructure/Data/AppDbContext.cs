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

        // TODO: Add DbSet<> properties for your domain entities.
        // Example:
        // public DbSet<Post> Posts => Set<Post>();
        // public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TODO: Apply entity configurations here.
            // builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }

