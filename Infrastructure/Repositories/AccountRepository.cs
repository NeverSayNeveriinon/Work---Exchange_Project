using Core.Domain.IdentityEntities;
using Core.Domain.RepositoryContracts;
using Infrastructure.DatabaseContext;

namespace Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _dbContext;

    public AccountRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    
    public void LoadReferences(UserProfile userProfile)
    {
        // _dbContext.Set<UserProfile>().Entry(userProfile).Reference(c => c.CurrencyAccounts).Load();
        _dbContext.Entry(userProfile).Collection(c => c.CurrencyAccounts).Load();
        _dbContext.Entry(userProfile).Collection(c => c.DefinedCurrencyAccounts).Load();
    }
}