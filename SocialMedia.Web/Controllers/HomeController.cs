using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Web.Models;
using System.Diagnostics;

namespace SocialMedia.Web.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IPostService _postService;
    public HomeController(ILogger<HomeController> logger , IPostService postService)
    {
        _logger = logger;
        this._postService = postService;
    }

    public async Task<IActionResult> Index()
    {
        var posts = await _postService.GetAllPostsAsync(); 
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
