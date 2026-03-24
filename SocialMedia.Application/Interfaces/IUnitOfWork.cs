using SocialMedia.Domain.Common;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Application.Interfaces
{
    public interface IUnitOfWork
    {
        // ── Generic repo for simple CRUD (BaseEntity descendants only) ────────
        IGenericRepository<T> Repository<T>() where T : BaseEntity;

        // ── Dev4 custom repos ─────────────────────────────────────────────────
        
        IMessageRepository Messages { get; }
        IGroupChatRepository GroupChats { get; }

        // ── User lookup — ApplicationUser extends IdentityUser, not BaseEntity,
        //    so it cannot go through Repository<T>. This method handles it directly.
        Task<ApplicationUser?> FindUserByIdAsync(string userId);


        // ── Commit all pending changes in one transaction ─────────────────────
        int Complete();
        Task<int> CompleteAsync();
    }
}
