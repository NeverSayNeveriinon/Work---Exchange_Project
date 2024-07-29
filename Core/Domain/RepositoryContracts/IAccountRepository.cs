using Core.Domain.IdentityEntities;

namespace Core.Domain.RepositoryContracts;

public interface IAccountRepository
{ 
    void LoadReferences(UserProfile userProfile);
}