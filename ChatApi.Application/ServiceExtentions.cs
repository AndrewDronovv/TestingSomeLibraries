using ChatApi.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApi.Application;

public static class ServiceExtentions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        services.AddControllers();

        return services;
    }
}
