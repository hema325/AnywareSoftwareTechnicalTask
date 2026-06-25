using Infrastructure.Authentication;
using Infrastructure.Authentication.Settings;
using Infrastructure.BackgroundJobs.TaskProcessingJob;
using Infrastructure.BackgroundJobs.TaskProcessingJob.Settings;
using Infrastructure.Caching;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Interceptors;
using Infrastructure.Persistance.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            // database
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlConnection"));
                options.AddInterceptors(sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>());
                options.AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>());

            });

            services
                .AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>())
                .AddScoped<AppDbContextInitializer>()
                .AddScoped<UpdateAuditableEntitiesInterceptor>()
                .AddScoped<PublishDomainEventsInterceptor>();

            services.Configure<SeedSettings>(configuration.GetSection(SeedSettings.SectionName));

            // authentication
            services
                .AddScoped<IPasswordHasher, PasswordHasherService>()
                .AddScoped<ICurrentUser, CurrentUserService>()
                .AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            // background jobs
            services.AddHostedService<TaskProcessingWorker>();
            services.AddSingleton<ITaskQueue, TaskQueue>();

            services.Configure<TaskWorkerSettings>(configuration.GetSection(TaskWorkerSettings.SectionName));

            // cache
            services
            .AddScoped<ICache, RedisCacheService>()
                .AddSingleton<IConnectionMultiplexer>(_ =>
                {
                    var connectionString = configuration.GetConnectionString("RedisConnection");
                    return ConnectionMultiplexer.Connect(connectionString);
                });

            return services;
        }

        public static async Task InitializeDBAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
            await initializer.InitializeAsync();
        }

    }
}
