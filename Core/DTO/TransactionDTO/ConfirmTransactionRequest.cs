using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.DTO.TransactionDTO;

public class ConfirmTransactionRequest
{
    [Required(ErrorMessage = "The 'TransactionId' Can't Be Blank!!!")]
    public Guid TransactionId { get; set; }
    
    [AllowedValues("Confirmed","Cancelled", ErrorMessage = "The Value Must be 'Confirmed' or 'Cancelled'")]
    [Required(ErrorMessage = "The 'TransactionStatus' Can't Be Blank!!!")]
    public string TransactionStatus { get; set; }
}