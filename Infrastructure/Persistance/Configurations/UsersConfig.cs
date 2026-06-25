using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configurations
{
    internal class UsersConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Email).IsUnique();

            builder.HasMany(u => u.TaskItems).WithOne(t => t.Owner)
                .HasForeignKey(t => t.OwnerId).OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.RefreshTokens).WithOne(t => t.User)
                .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
