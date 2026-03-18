using SocialMedia.Domain.Common;
using SocialMedia.Application.Interfaces;
using SocialMedia.Infrastructure.Repositories;

namespace SocialMedia.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repos = [];

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            Type type = typeof(T);

            if (!_repos.TryGetValue(type, out var repo))
            {
                // I made a Repository class in Infrastructure.Repositories to solve the error in this line
                repo = new Repository<T>(_context);
                _repos[type] = repo;
            }

            return (IRepository<T>)repo;
        }
    }
}