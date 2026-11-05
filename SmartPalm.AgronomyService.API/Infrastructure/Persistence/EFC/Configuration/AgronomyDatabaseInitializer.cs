using Microsoft.EntityFrameworkCore;
namespace SmartPalm.AgronomyService.Infrastructure.Persistence;
public static class AgronomyDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    { using var scope = services.CreateScope(); await scope.ServiceProvider.GetRequiredService<AgronomyDbContext>().Database.MigrateAsync(); }
}

