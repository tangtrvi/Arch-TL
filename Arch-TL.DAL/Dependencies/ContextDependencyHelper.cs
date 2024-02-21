using Arch_TL.DAL.Context;
using Arch_TL.DAL.Context.Interfaces;
using Arch_TL.DAL.Context.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arch_TL.DAL.Dependencies;

public static class ContextDependencyHelper
{
    public static IServiceCollection AddContext(this IServiceCollection services)
    {
        services.AddSingleton<IQueryOrm, QueryOrm>();
        services.AddContextScope();

        return services;
    }

    public static IServiceCollection AddContextScope(this IServiceCollection services)
    {
        services.AddScoped<ILoggingRepository, LoggingRepository>();
        services.AddScoped<IBiologyDomainRepository, BiologyDomainRepository>();

        return services;
    }
}
