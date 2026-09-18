using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using UserService.Enums;

namespace UserService.Models;

public class User
{
    [Key]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    [MinLength(8)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public Role Role { get; set; } = Role.Patron;
    
    public MembershipStatus MembershipStatus { get; set; } =  MembershipStatus.Active;
    
    public DateTime? MemberSince { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}