using System.ComponentModel.DataAnnotations;

namespace Core.DTO.Auth;

public class UserLogin
{
    [Required(ErrorMessage = "The 'Email' Can't Be Blank!!!")]
    [StringLength(60, ErrorMessage = "The 'Email' Can't Be More Than 60 Characters")]
    [EmailAddress(ErrorMessage = "The 'Email' is not in a Correct Format")]
    public string Email { get; set; }

   
    [Required(ErrorMessage = "Password can't be blank")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}