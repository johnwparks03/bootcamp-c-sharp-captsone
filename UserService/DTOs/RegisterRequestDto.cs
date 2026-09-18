using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage =  "Email must be a valid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [MaxLength(255, ErrorMessage =  "Password can not exceed 255 characters")]
    [MinLength(8,  ErrorMessage =  "Password must be at least 8 characters")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":{}|<>]).{8,}$",
        ErrorMessage = "Pasword must include an uppercase letter, lowercase letter, number, and a special character."
    )]
    public string Password { get; set; }
    
    [Required(ErrorMessage =  "First name is required")]
    [MaxLength(100, ErrorMessage =  "First name can not exceed 100 characters")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage =  "Last name can not exceed 100 characters")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage =  "Phone number is required")]
    [MaxLength(20, ErrorMessage =  "Phone number is too long")]
    [Phone(ErrorMessage = "Phone number must be a valid phone number")]
    public string PhoneNumber { get; set; }
}