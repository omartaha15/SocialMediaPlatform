using SocialMedia.Domain.Common;
using SocialMedia.Infrastructure.Data;
using SocialMedia.Application.Interfaces;

namespace SocialMedia.Infrastructure.Repositories
{
    // I made this file to fix the error on line 34 in UnitOfWork class
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;
        public Repository(AppDbContext context) 
        {
            _context = context;
        }

        public Task AddAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }
    }
}