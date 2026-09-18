using UserService.DTOs;
using UserService.Enums;
using UserService.Exceptions;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUserExists = await _userRepository.ExistsByEmailAsync(request.Email);
        if (existingUserExists)
        {
            throw new EmailAlreadyExistsException();
        }
        
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = hashedPassword,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = Role.Patron,
            MembershipStatus = MembershipStatus.Active,
        };
        
        user = await _userRepository.CreateAsync(user);

        _logger.LogInformation($"Register successful for : {user.Email}");
        return new RegisterResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            MembershipStatus = user.MembershipStatus,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            _logger.LogInformation($"Failed login attempt");
            throw new LoginFailedException();
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogInformation($"Failed login attempt");
            throw new LoginFailedException();
        }
        
        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation($"Login successful for : {user.Email}");
        return new LoginResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 86400,
            User = new LoginUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
            }
        };
        
    }

}