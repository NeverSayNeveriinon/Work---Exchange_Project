using Core.Domain.IdentityEntities;

namespace Core.Domain.RepositoryContracts;

public interface IAccountRepository
{
    public void LoadReferences(UserProfile userProfile);
}