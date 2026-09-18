using System.Text.Json.Serialization;
using UserService.Enums;

namespace UserService.DTOs;

public class LoginResponseDto
{
    [property: JsonPropertyName("accessToken")]public string AccessToken { get; set; }
    [property: JsonPropertyName("tokenType")]public string TokenType { get; set; } = "Bearer";
    [property: JsonPropertyName("expiresIn")]public int ExpiresIn { get; set; } = 86400;
    [property: JsonPropertyName("user")]public LoginUserDto User { get; set; }
}
public class LoginUserDto
{
    [property: JsonPropertyName("userId")]public Guid UserId { get; set; }
    [property: JsonPropertyName("email")]public string Email { get; set; }
    [property: JsonPropertyName("firstName")]public string FirstName { get; set; }
    [property: JsonPropertyName("lastName")]public string LastName { get; set; }
    [property: JsonPropertyName("role")]public Role Role { get; set; }
}