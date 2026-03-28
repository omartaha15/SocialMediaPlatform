using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.Interfaces.Services;

namespace SocialMedia.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public AdminPanelController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var analytics = await _dashboardService.GetAnalyticsSummaryAsync();
            return View(analytics);
        }
    }
}
