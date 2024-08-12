using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using API.Helpers;
using ConnectingApps.SmartInject;
using Core.Domain.ExternalServicesContracts;
using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Core.Helpers;
using Core.ServiceContracts;
using Core.Services;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Core;
using IdempotentAPI.Extensions.DependencyInjection;
using Infrastructure.DatabaseContext;
using Infrastructure.ExternalServices;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API;

// TODO: Add Exception Middleware
// TODO: Add Eager,Lazy Loading
// TODO: Add AntiforgeryToken (CSRF)
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers()
                        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        // builder.Services.AddControllersWithViews(options =>
        // {
        //     options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        // });

        // Services IOC        
        builder.Services.RegisterServices();
        
        // DataBase IOC
        var DBconnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(DBconnectionString);
        });
        
        // Identity IOC
        builder.Services.AddIdentity<UserProfile, UserRole>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 3; //Eg: AB12AB
                options.SignIn.RequireConfirmedEmail = true;
                // options.Tokens.EmailConfirmationTokenProvider = "Default";
                // options.Tokens.ProviderMap.Add("Default",
                //     new TokenProviderDescriptor(typeof(IUserTwoFactorTokenProvider<UserProfile>))
                //     {
                //         
                //     }
            }).AddEntityFrameworkStores<AppDbContext>()
            .AddUserStore<UserStore<UserProfile, UserRole, AppDbContext, Guid>>()
            .AddRoleStore<RoleStore<UserRole, AppDbContext, Guid>>()
            .AddDefaultTokenProviders();
        
        // JWT
        builder.Services.AddAuthentication(options => 
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });
        
        // Swagger
        // Generates description for all endpoints (action methods)
        builder.Services.AddEndpointsApiExplorer(); 
        // Generates OpenAPI specification
        builder.Services.AddSwaggerGen(options =>
        {
            options.IncludeXmlComments("wwwroot/ExchangeApp.xml"); // For Reading the 'XML' comments
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please Enter JWT Token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        }); 
        
        // Authorization
        // configure a policy to authorization
        builder.Services.AddAuthorization(options =>
        {
            // enforces authorization policy (user must be authenticated) for all the action methods
            var policyBuilder = new AuthorizationPolicyBuilder();
            var policy = policyBuilder.RequireAuthenticatedUser().Build(); 
            options.FallbackPolicy = policy;
            
            // add a custom policy to be used in 'AccountController'
            options.AddPolicy("NotAuthorized", custompolicy =>
            {
                custompolicy.RequireAssertion(context =>
                {
                    return !context.User.Identity?.IsAuthenticated ?? false;
                });
            });
        });

        builder.Services.AddIdempotentAPI(new IdempotencyOptions());
        builder.Services.AddDistributedMemoryCache(); 
        builder.Services.AddIdempotentAPIUsingDistributedCache();

        builder.Services.AddMemoryCache();
        
        // builder.Services.AddHsts(options =>
        // {
        //     options.ExcludedHosts.Clear();
        //     options.Preload = true;
        //     options.IncludeSubDomains = true;
        //     options.MaxAge = TimeSpan.FromDays(60);
        // });
        
        var app = builder.Build();
        
        app.EnsureCreatingDatabase<AppDbContext>();
        
        // Middlewares //
            
        app.UseHsts();
        app.UseHttpsRedirection();
        
        // Swagger
        app.UseSwagger(); // Creates endpoints for swagger.json
        app.UseSwaggerUI(); // Creates swagger UI for testing all endpoints (action methods)
        
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication(); 
        app.UseAuthorization(); 
        app.MapControllers();

        app.Run();
    }
}