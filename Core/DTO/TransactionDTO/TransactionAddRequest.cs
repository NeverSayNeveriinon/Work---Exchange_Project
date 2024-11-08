﻿using Core.Domain.Entities;
using Core.Domain.IdentityEntities;
using Core.Enums;

namespace Core.DTO.TransactionDTO;

public class TransactionAddRequest
{
    public decimal Amount { get; set; }
    public int FromAccountNumber { get; set; }
    public int ToAccountNumber { get; set; }
}

public static partial class TransactionExtensions
{
    public static Transaction ToTransaction(this TransactionAddRequest transactionAddRequest)
    {
        Transaction transaction = new Transaction()
        {
            Id = 0,
            Amount = 0,
            FromAccountNumber = transactionAddRequest.FromAccountNumber,
            ToAccountNumber = transactionAddRequest.ToAccountNumber,
            DateTime = DateTime.Now
        };

        return transaction;
    }
}