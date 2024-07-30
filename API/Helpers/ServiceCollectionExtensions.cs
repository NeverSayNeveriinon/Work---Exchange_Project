using ConnectingApps.SmartInject;
using Core.Domain.ExternalServicesContracts;
using Core.Domain.RepositoryContracts;
using Core.Helpers;
using Core.ServiceContracts;
using Core.Services;
using Infrastructure.ExternalServices;
using Infrastructure.Repositories;

namespace API.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<INotificationService, EmailService>();
        
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        
        services.AddScoped<ICurrencyAccountRepository, CurrencyAccountRepository>();
        services.AddScoped<ICurrencyAccountService, CurrencyAccountService>();
        
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        
        services.AddScoped<ICommissionRateRepository, CommissionRateRepository>();
        services.AddScoped<ICommissionRateService, CommissionRateService>();

        services.AddScoped<IExchangeValueRepository, ExchangeValueRepository>();
        services.AddScoped<IExchangeValueService, ExchangeValueService>();
        
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddLazyScoped<ITransactionService, TransactionService>();

        services.AddScoped<IValidator, Validator>();

        return services;
    }
}