using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        // DataBase IOC
        var DBconnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(DBconnectionString);
        });
        
        
        var app = builder.Build();

        app.UseHsts();
        app.UseHttpsRedirection();
        
        
        app.UseStaticFiles();
        
        app.MapControllers();

        app.Run();
    }
}