using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.DTOs.AdminDTOs
{
    public class DashboardAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalGroups { get; set; }
        public int TotalComments { get; set; }
        public int TotalReactions { get; set; }
        public int TotalMessages { get; set; }
    }
}
