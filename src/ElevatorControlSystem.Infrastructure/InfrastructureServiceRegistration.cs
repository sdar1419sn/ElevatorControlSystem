using ElevatorControlSystem.Domain.Repositories;
using ElevatorControlSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElevatorControlSystem.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ElevatorDbContext>(options => options.UseInMemoryDatabase("ElevatorDb"));
        services.AddScoped<IElevatorRepository, ElevatorRepository>();
        services.AddScoped<IFloorRequestRepository, FloorRequestRepository>();
        return services;
    }
}