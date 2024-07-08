using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.Domain.Entities;

public class DefinedAccount
{
    public int Number { get; set; }
    public OwnershipTypeOptions OwnershipType { get; set; }
}