using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configurations
{
    internal class RefreshTokensConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasIndex(t => t.Token).IsUnique();

            builder.HasQueryFilter(t => !t.User.IsDeleted);

            builder.Property(t => t.Token)
                .HasMaxLength(200);
        }
    }
}
