using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTOs.AdminDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Services;
using SocialMedia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Application.Services
{
    public class DashboardService:IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<DashboardAnalyticsDto> GetAnalyticsSummaryAsync()
        {
         
            int usersCount = await _userManager.Users.CountAsync();

          
            int postsCount = await _unitOfWork.Repository<Post>().Query().CountAsync();
            int groupsCount = await _unitOfWork.Repository<Group>().Query().CountAsync();
            int commentsCount = await _unitOfWork.Repository<Comment>().Query().CountAsync();

          
            int reactionsCount = await _unitOfWork.Repository<Reaction>().Query().CountAsync();
            int messagesCount = await _unitOfWork.Repository<Message>().Query().CountAsync();

         
            return new DashboardAnalyticsDto
            {
                TotalUsers = usersCount,
                TotalPosts = postsCount,
                TotalGroups = groupsCount,
                TotalComments = commentsCount,
                TotalReactions = reactionsCount,
                TotalMessages = messagesCount
            };
        }
    }
}
