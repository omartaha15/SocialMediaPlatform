using SocialMedia.Application.DTOs.AdminDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardAnalyticsDto> GetAnalyticsSummaryAsync();
    }
}
