using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}