using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Web.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace SocialMedia.Web.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPostService _postService;
    private readonly IReactionService _reactionService;

    public HomeController(ILogger<HomeController> logger, IPostService postService, IReactionService reactionService)
    {
        _logger = logger;
        _postService = postService;
        _reactionService = reactionService;
    }

    public async Task<IActionResult> Index()
    {
        var posts = (await _postService.GetAllPostsAsync()).ToList();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // Sequential: DbContext is NOT thread-safe, never use Task.WhenAll here
        foreach (var post in posts)
        {
            var (counts, userReaction) = await _reactionService.GetReactionSummaryAsync(post.Id, userId);
            post.ReactionCounts = counts;
            post.CurrentUserReaction = userReaction;
            post.ReactionCount = counts.Values.Sum();
        }

        return View(posts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
