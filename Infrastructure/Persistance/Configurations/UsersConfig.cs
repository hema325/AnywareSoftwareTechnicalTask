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
        }
    }
}
