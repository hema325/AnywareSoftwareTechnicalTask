using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistance.Interceptors
{
    public class UpdateAuditableEntitiesInterceptor: SaveChangesInterceptor
    {
        private readonly ICurrentUser _currentUser;

        public UpdateAuditableEntitiesInterceptor(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            if (context is null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var dateTime = DateTime.UtcNow;
            var userId = _currentUser.Id;

            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = dateTime;
                        entry.Entity.CreatedBy = userId;
                        break;

                    case EntityState.Modified:
                        entry.Property(x => x.CreatedAt).IsModified = false;
                        entry.Property(x => x.CreatedBy).IsModified = false;

                        entry.Entity.UpdatedAt = dateTime;
                        entry.Entity.UpdatedBy = userId;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;

                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = dateTime;
                        entry.Entity.DeletedBy = userId;
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
