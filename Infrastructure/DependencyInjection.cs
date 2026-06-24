using Infrastructure.Authentication;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            // database
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services
                .AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>())
                .AddScoped<AppDbContextInitializer>();

            services.Configure<SeedSettings>(configuration.GetSection(SeedSettings.SectionName));

            // authentication
            services
                .AddScoped<IPasswordHasher, PasswordHasherService>()
                .AddScoped<ICurrentUser, CurrentUserService>();
            
            return services;
        }

        public static async Task InitializeDBAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
                await initializer.InitializeAsync();
            }
        }

    }
}
