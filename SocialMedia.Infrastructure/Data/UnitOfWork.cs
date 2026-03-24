using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.Interfaces;
using SocialMedia.Application.Interfaces.Repositories;
using SocialMedia.Domain.Common;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Repositories;

namespace SocialMedia.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repos = [];

        // ── Dev4 custom repos — lazy-initialised ─────────────────────────────
        private IMessageRepository? _messages;
        private IGroupChatRepository? _groupChats;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // ── Custom repo properties ────────────────────────────────────────────
        public IMessageRepository Messages
            => _messages ??= new MessageRepository(_context);

        public IGroupChatRepository GroupChats
            => _groupChats ??= new GroupChatRepository(_context);

        // ── Generic repo (BaseEntity descendants only) ────────────────────────
        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T);
            if (!_repos.TryGetValue(type, out var repo))
            {
                repo = new Repository<T>(_context);
                _repos[type] = repo;
            }
            return (IGenericRepository<T>)repo;
        }

        // ── User lookup — ApplicationUser extends IdentityUser, not BaseEntity ─
        public async Task<ApplicationUser?> FindUserByIdAsync(string userId)
            => await _context.Users.FindAsync(userId);

        // ── Single commit point for ALL pending changes ───────────────────────
        public int Complete()
            => _context.SaveChanges();

        public async Task<int> CompleteAsync()
            => await _context.SaveChangesAsync();
    }
}
