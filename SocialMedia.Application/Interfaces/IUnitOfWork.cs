using SocialMedia.Domain.Common;

namespace SocialMedia.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<T> Repository<T>() where T : BaseEntity;
        int Complete();
        Task<int> CompleteAsync();
    }
}