using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using task_crud.Application.Contracts;
using task_crud.Domain.Repositories;
using task_crud.Infrastructure.Data;
using task_crud.Infrastructure.Repository;
using task_crud.Infrastructure.Services;

namespace task_crud.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext(configuration);
            services.AddRepositories(configuration);
            services.AddServices(configuration);
            services.AddHttpClient();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITodoService, TodoService>();
            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITodoRepository, TodoRepository>();
            return services;
        }

        private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("Database");
                options.UseSqlServer(connectionString);
            });
            return services;
        }
    }
}
