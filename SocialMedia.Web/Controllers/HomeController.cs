using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Web.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace SocialMedia.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPostService _postService;
    private readonly IReactionService _reactionService;
    private readonly IGitHubService _githubService;
    private readonly ICompositeViewEngine _viewEngine;

    private const int PageSize = 5;

    public HomeController(ILogger<HomeController> logger, 
        IPostService postService, 
        IReactionService reactionService, 
        ICompositeViewEngine viewEngine,
        IGitHubService githubService)
    {
        _logger = logger;
        _postService = postService;
        _reactionService = reactionService;
        _viewEngine = viewEngine;
        _githubService = githubService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var (posts, hasMore) = await _postService.GetPagedPostsAsync(1, PageSize);
        var postList = posts.ToList();

        // Integrate GitHub Updates
        try
        {
            var commits = await _githubService.GetRecentCommitsAsync("omartaha15", "SocialMediaPlatform", 3);
            foreach (var commit in commits)
            {
                postList.Add(new Application.DTOs.PostDtos.PostDto
                {
                    Id = Guid.NewGuid(),
                    Content = $"🛠️ GitHub Commit: {commit.Message}",
                    UserName = commit.AuthorName,
                    UserAvatarUrl = commit.AuthorAvatarUrl,
                    CreatedAt = commit.CommittedAt,
                    IsGitHubUpdate = true,
                    GitHubUrl = commit.HtmlUrl
                });
            }
        }
        catch { /* Fallback */ }

        foreach (var post in postList.Where(p => !p.IsGitHubUpdate))
        {
            var (counts, userReaction) = await _reactionService.GetReactionSummaryAsync(post.Id, userId);
            post.ReactionCounts = counts;
            post.CurrentUserReaction = userReaction;
            post.ReactionCount = counts.Values.Sum();
        }

        ViewBag.HasMorePosts = hasMore;
        ViewBag.CurrentPage = 1;
        
        return View(postList.OrderByDescending(p => p.CreatedAt).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> LoadMorePosts(int page = 2)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var (posts, hasMore) = await _postService.GetPagedPostsAsync(page, PageSize);
        var postList = posts.ToList();

        var htmlResults = new List<string>();
        foreach (var post in postList)
        {
            var (counts, userReaction) = await _reactionService.GetReactionSummaryAsync(post.Id, userId);
            post.ReactionCounts = counts;
            post.CurrentUserReaction = userReaction;
            post.ReactionCount = counts.Values.Sum();
            
            htmlResults.Add(await RenderPartialViewToString("_PostPartial", post));
        }

        return Json(new
        {
            hasMore,
            page,
            html = string.Join("", htmlResults)
        });
    }

    private async Task<string> RenderPartialViewToString(string viewName, object model)
    {
        ViewData.Model = model;
        using (var sw = new StringWriter())
        {
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

            if (viewResult.View == null)
            {
                // Try fallback for shared
                viewResult = _viewEngine.GetView(null, $"~/Views/Shared/{viewName}.cshtml", false);
            }

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.GetStringBuilder().ToString();
        }
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
