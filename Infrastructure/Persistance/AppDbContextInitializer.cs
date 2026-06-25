using Infrastructure.Authentication;
using Infrastructure.Persistance.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistance
{
    internal class AppDbContextInitializer
    {
        private readonly AppDbContext _context;
        private readonly SeedSettings _seedSettings;
        private readonly IPasswordHasher _passwordHasher;

        public AppDbContextInitializer(AppDbContext context, IOptions<SeedSettings> options, IPasswordHasher passwordHasher)
        {
            _context = context;
            _seedSettings = options.Value;
            _passwordHasher = passwordHasher;
        }

        public async Task InitializeAsync()
        {
            if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                await _context.Database.MigrateAsync();
            }

            if (!await _context.Users.AnyAsync())
            {
                var users = _seedSettings.Users.Select(user => new User
                {
                    Name = user.Name,
                    Email = user.Email,
                    HashedPassword = _passwordHasher.HashPassword(user.Password),
                    Role = user.Role
                });

                await _context.Users.AddRangeAsync(users);
                await _context.SaveChangesAsync();
            }
        }
    }
}
