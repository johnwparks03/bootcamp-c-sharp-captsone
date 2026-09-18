using UserService.Models;

namespace UserService.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    string? GetEmailFromToken(string token);
}