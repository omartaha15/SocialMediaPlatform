using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Web.Hubs;

namespace SocialMedia.Web.Services
{
    public class DashboardNotifierService: IDashboardNotifierService
    {
        private readonly IHubContext<AdminDashboardHub> _hubContext;
        private readonly IDashboardService _dashboardService;

        public DashboardNotifierService(
            IHubContext<AdminDashboardHub> hubContext,
            IDashboardService dashboardService)
        {
            _hubContext = hubContext;
            _dashboardService = dashboardService;
        }

        public async Task NotifyDashboardUpdatedAsync()
        {
          
            var data = await _dashboardService.GetAnalyticsSummaryAsync();

          
            await _hubContext.Clients
                .Group("Admins")
                .SendAsync("UpdateDashboard", data);
        }
    }
}
