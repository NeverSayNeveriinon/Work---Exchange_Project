using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public static class WebApplicationExtensions
{
    public static async void EnsureCreatingDatabase<T>(this WebApplication app) where T : DbContext
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetService<T>();
            if (await context!.Database.CanConnectAsync())
            {
                // if (context!.Database.GetPendingMigrations().Any())
                // {
                //     await context.Database.MigrateAsync();
                // }
            }

            else
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
            }
        }
    }
}