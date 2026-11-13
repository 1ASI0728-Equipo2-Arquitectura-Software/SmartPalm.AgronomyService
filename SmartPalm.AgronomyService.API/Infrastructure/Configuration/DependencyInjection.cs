using Microsoft.EntityFrameworkCore;
using SmartPalm.AgronomyService.Application.CommandServices;
using SmartPalm.AgronomyService.Application.DomainServices;
using SmartPalm.AgronomyService.Application.EventHandlers;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Application.QueryServices;
using SmartPalm.AgronomyService.Application.Services;
using SmartPalm.AgronomyService.Domain.Repositories;
using SmartPalm.AgronomyService.Domain.Services;
using SmartPalm.AgronomyService.Infrastructure.Configuration;
using SmartPalm.AgronomyService.Infrastructure.ExternalServices;
using SmartPalm.AgronomyService.Infrastructure.Messaging;
using SmartPalm.AgronomyService.Infrastructure.Persistence;

namespace SmartPalm.AgronomyService.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddAgronomyInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        var connection = configuration.GetConnectionString("AgronomyDatabase") ?? throw new InvalidOperationException("ConnectionStrings:AgronomyDatabase is required.");
        services.AddDbContext<AgronomyDbContext>(o => o.UseNpgsql(connection).UseSnakeCaseNamingConvention()); services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IRecommendationRepository, RecommendationRepository>(); services.AddScoped<ITechnicalReportRepository, TechnicalReportRepository>();
        services.AddScoped<IAgronomyProjectionRepository, AgronomyProjectionRepository>(); services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<AgronomyDbContext>());
        services.AddScoped<IIntegrationEventWriter, IntegrationEventWriter>(); services.AddScoped<IInbox, Inbox>(); services.AddScoped<IntegrationEventHandlers>();
        services.AddScoped<AgronomyAuthorizationService>();
        services.AddHttpClient<IDiagnosisProvider, MiniMaxDiagnosisProvider>(client => client.Timeout = TimeSpan.FromSeconds(60));
        services.AddOptions<ReconciliationOptions>().BindConfiguration(ReconciliationOptions.SectionName);
        services.AddHttpClient<IIdentitySnapshotClient, IdentitySnapshotClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ReconciliationOptions>>().Value;
            client.BaseAddress = new Uri(options.IdentityBaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(60);
        });
        services.AddHttpClient<ICropSnapshotClient, CropSnapshotClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ReconciliationOptions>>().Value;
            client.BaseAddress = new Uri(options.CropBaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(60);
        });
        services.AddScoped<AgronomyProjectionReconciliationService>();
        services.AddScoped<IRecommendationCommandService, RecommendationCommandService>(); services.AddScoped<IRecommendationQueryService, RecommendationQueryService>();
        services.AddScoped<IReportCommandService, ReportCommandService>(); services.AddScoped<IReportQueryService, ReportQueryService>(); services.AddSingleton<AgronomyAccessPolicy>();
        services.AddHostedService<IntegrationEventConsumer>(); services.AddHostedService<OutboxPublisher>(); return services;
    }
}
