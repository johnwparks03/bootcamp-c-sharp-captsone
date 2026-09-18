using System.Text.Json.Serialization;
using UserService.Enums;

namespace UserService.DTOs;

public class ValidatedUserDto
{
    [property: JsonPropertyName("userId")]public Guid UserId { get; set; }
    [property: JsonPropertyName("email")]public string Email { get; set; }
    [property: JsonPropertyName("firstName")]public string FirstName { get; set; }
    [property: JsonPropertyName("lastName")]public string LastName { get; set; }
    [property: JsonPropertyName("role")]public Role Role { get; set; }
    [property: JsonPropertyName("membershipStatus")]public MembershipStatus MembershipStatus { get; set; }
    [property: JsonPropertyName("activeReservations")]public int ActiveReservations { get; set; }
}