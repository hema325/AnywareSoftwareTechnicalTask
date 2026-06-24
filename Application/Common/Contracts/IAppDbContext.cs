using Microsoft.EntityFrameworkCore;

namespace Application.Common.Contracts
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<TaskItem> TaskItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
