using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTOs.GroupChatDTOs;
using SocialMedia.Application.Interfaces;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using SocialMedia.Infrastructure.Data;

namespace SocialMedia.Application.Services
{
    public class GroupChatService : IGroupChatService
    {
        private readonly AppDbContext _context;

        public GroupChatService(AppDbContext context)
        {
            _context = context;
        }

        // ── Create Group ────────────────────────────────────────────────────
        public async Task<GroupDto> CreateGroupAsync(string creatorId, CreateGroupDto dto)
        {
            var group = new Group
            {
                Name        = dto.Name,
                Description = dto.Description,
                CreatedAt   = DateTime.UtcNow
            };

            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

