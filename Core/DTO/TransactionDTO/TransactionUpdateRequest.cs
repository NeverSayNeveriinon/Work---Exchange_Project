using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

public class TransactionUpdateRequest
{

}

public static partial class TransactionExtensions
{
    public static Transaction ToTransaction(this TransactionUpdateRequest transactionUpdateRequest)
    {
        Transaction transaction = new Transaction()
        {

        };

        return transaction;
    }
}