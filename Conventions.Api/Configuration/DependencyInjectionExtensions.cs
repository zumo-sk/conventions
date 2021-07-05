namespace Conventions.Api.Configuration
{
    using Conventions.Domain.DataAccess;
    using Conventions.Domain.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInjections(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDomainDataAccess(configuration).AddDomainServices();

        private static IServiceCollection AddDomainDataAccess(this IServiceCollection services, IConfiguration configuration) =>
            services.AddSingleton<IConnectionFactory>(new ConnectionFactory(configuration.GetConnectionString("FileName")))
                    .AddScoped<IVenueDataStore, VenueDataStore>()
                    .AddScoped<IConventionDataStore, ConventionDataStore>()
                    .AddScoped<IUserDataStore, UserDataStore>()
                    .AddScoped<ITalkDataStore, TalkDataStore>()
                    .AddScoped<IConventionRegistrationDataStore, ConventionRegistrationDataStore>()
                    .AddScoped<ITalkRegistrationDataStore, TalkRegistrationDataStore>();

        private static IServiceCollection AddDomainServices(this IServiceCollection services) =>
            services.AddScoped<IVenueService, VenueService>()
                    .AddScoped<IConventionService, ConventionService>()
                    .AddScoped<IUserService, UserService>()
                    .AddScoped<ITalkService, TalkService>();
    }
}
