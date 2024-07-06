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
        
        // Swagger
        // Generates description for all endpoints (action methods)
        builder.Services.AddEndpointsApiExplorer(); 
        // Generates OpenAPI specification
        builder.Services.AddSwaggerGen(options =>
        {
            // options.IncludeXmlComments("wwwroot/ExchangeApp.xml"); // For Reading the 'XML' comments
        }); 
        
        var app = builder.Build();

        app.UseHsts();
        app.UseHttpsRedirection();
        
        // Swagger
        app.UseSwagger(); // Creates endpoints for swagger.json
        app.UseSwaggerUI(); // Creates swagger UI for testing all endpoints (action methods)
        
        app.UseStaticFiles();
        
        app.MapControllers();

        app.Run();
    }
}