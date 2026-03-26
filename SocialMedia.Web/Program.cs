using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Repositories;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Application.Services;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;
using SocialMedia.Infrastructure.Repositories;
using SocialMedia.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath         = "/Account/Login";
    options.LogoutPath        = "/Account/Logout";
    options.ExpireTimeSpan    = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ── Infrastructure ────────────────────────────────────────────────────────────
// UnitOfWork owns IMessageRepository and IGroupChatRepository internally.
// Only repos that are NOT inside UnitOfWork are registered here explicitly.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>(); // Dev3 — not in UoW
builder.Services.AddScoped<IUserSearchRepository, UserSearchRepository>();

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IGroupChatService, GroupChatService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddScoped<IPostService, PostService>();  
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddSignalR();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
       
        await SocialMedia.Infrastructure.Data.DataSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();
