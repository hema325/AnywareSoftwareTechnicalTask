using System.Data;
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
            if (_context.Database.GetPendingMigrations().Any())
            {
                await _context.Database.MigrateAsync();
            }

            if (!await _context.Users.AnyAsync())
            {
                await SeedUsersAsync();
            }

            if (!await _context.TaskItems.AnyAsync())
            {
                await SeedTaskItemsAsync();
            }
        }

        private async Task SeedTaskItemsAsync()
        {
            var taskItems = _seedSettings.TaskItems.Select(taskItem => new TaskItem
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                Status = taskItem.Status,
                Priority = taskItem.Priority,
                CreatedAt = taskItem.CreatedAt,
                OwnerId = taskItem.OwnerId
            });

            await _context.TaskItems.AddRangeAsync(taskItems);
            await SaveChangesWithIdentityAsync(nameof(AppDbContext.TaskItems));
        }

        private async Task SeedUsersAsync()
        {
            var users = _seedSettings.Users.Select(user => new User
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                HashedPassword = _passwordHasher.HashPassword(user.Password),
                Role = user.Role,
                CreatedAt = user.CreatedAt
            });

            await _context.Users.AddRangeAsync(users);
            await SaveChangesWithIdentityAsync(nameof(AppDbContext.Users));
        }

        private async Task SaveChangesWithIdentityAsync(string tableName)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] ON");

            try
            {
                await _context.SaveChangesAsync();
            }
            finally
            {
                await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] OFF");
            }
        }
    }
}
