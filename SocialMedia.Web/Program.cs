using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Services;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;
using SocialMedia.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    )
    .AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; 
    options.LogoutPath = "/Account/Logout"; 
    //options.AccessDeniedPath = "/Account/AccessDenied"; 
    options.ExpireTimeSpan = TimeSpan.FromDays(7); 
    options.SlidingExpiration = true; 
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IImageService, ImageService>();

//builder.Services.AddAntiforgery(options =>
//{

//    options.Cookie.Name = ".SocialMedia.Antiforgery";
//});
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Map SignalR hubs
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

app.Run();
