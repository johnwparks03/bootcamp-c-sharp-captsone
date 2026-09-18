using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _userDbContext;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UserDbContext userDbContext, ILogger<UserRepository> logger)
    {
        _logger = logger;
        _userDbContext = userDbContext;
    }
    
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        _logger.LogInformation($"Getting user with id: {userId}");
        return await _userDbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        _logger.LogInformation($"Getting user with email: {email}");
        return await _userDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        _logger.LogInformation($"Creating user with email: {user.Email}");
        _userDbContext.Users.Add(user);
        await _userDbContext.SaveChangesAsync();
        return await _userDbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email) ?? user;
    }

    public async Task<bool> ExistsByIdAsync(Guid userId)
    {
        return await _userDbContext.Users.AnyAsync(u => u.UserId == userId);
    }
    
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _userDbContext.Users.AnyAsync(u => u.Email == email);
    }
}