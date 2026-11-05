using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Service.Helpers;
using ServiceAbstraction;
using Shared.Settings;

namespace Service
{
    public static class ApplicationServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IpSecurityService>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            services.AddHttpClient();

            services.AddAutoMapper((serviceProvider, cfg) =>
            {
                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();
                cfg.AddProfile(new MappingProfile(appSettings));
            }, typeof(MappingProfile));

            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddSingleton(TimeProvider.System);

            return services;
        }

    }
}
