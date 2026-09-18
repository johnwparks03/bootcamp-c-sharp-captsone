using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserService.DTOs;
using UserService.Enums;
using UserService.Exceptions;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;

namespace UserService.Tests.Unit;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsRegisteredUser()
    {
        var userRepository = new Mock<IUserRepository>();
        var jwtService = new Mock<IJwtService>();

        var validRequest = new RegisterRequestDto
        {
            Email = "patron@example.com",
            Password = "StrongPassword1!",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };
        
        var savedUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = validRequest.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(validRequest.Password),
            FirstName = validRequest.FirstName,
            LastName = validRequest.LastName,
            PhoneNumber = validRequest.PhoneNumber,
            Role = Role.Patron,
            MembershipStatus = MembershipStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        userRepository.Setup(x => x.ExistsByEmailAsync(validRequest.Email)).ReturnsAsync(false);
        
        userRepository.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(savedUser);

        var service = new AuthService(
            userRepository.Object,
            jwtService.Object,
            NullLogger<AuthService>.Instance
        );
        
        var result = await service.RegisterAsync(validRequest);

        // Assert
        Assert.Equal(savedUser.UserId, result.UserId);
        Assert.Equal(validRequest.Email, result.Email);
        Assert.Equal(validRequest.FirstName, result.FirstName);
        Assert.Equal(validRequest.LastName, result.LastName);
        Assert.Equal(Role.Patron, result.Role);
        Assert.Equal(MembershipStatus.Active, result.MembershipStatus);

        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                validRequest.Password,
                savedUser.PasswordHash));

        userRepository.Verify(
            x => x.ExistsByEmailAsync(validRequest.Email),
            Times.Once);

        userRepository.Verify(
            x => x.CreateAsync(It.Is<User>(user =>
                user.Email == validRequest.Email &&
                user.FirstName == validRequest.FirstName &&
                user.LastName == validRequest.LastName &&
                user.Role == Role.Patron &&
                user.MembershipStatus == MembershipStatus.Active &&
                BCrypt.Net.BCrypt.Verify(
                    validRequest.Password,
                    user.PasswordHash))),
            Times.Once);
    }    
    
    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsException()
    {
        var userRepository = new Mock<IUserRepository>();
        var jwtService = new Mock<IJwtService>();
        
        var request = new RegisterRequestDto
        {
            Email = "patron@example.com",
            Password = "StrongPassword1!",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };
        
        userRepository.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);
        
        var service = new AuthService(
            userRepository.Object,
            jwtService.Object,
            NullLogger<AuthService>.Instance
        );
        
        await Assert.ThrowsAsync<EmailAlreadyExistsException>(() => service.RegisterAsync(request));
        
        userRepository.Verify(
            x => x.CreateAsync(It.IsAny<User>()),
            Times.Never);
        
    }
    
    [Fact]
    public async Task LoginAsync_ValidRequest_ReturnsUser()
    {
        var userRepository = new Mock<IUserRepository>();
        var jwtService = new Mock<IJwtService>();

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("StrongPassword1!"),
            FirstName = "John",
            LastName = "Smith",
            Role = Role.Patron,
            MembershipStatus = MembershipStatus.Active,
        };

        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = "StrongPassword1!"
        };
        
        userRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        jwtService.Setup(x => x.GenerateToken(user)).Returns("fake-token");

        var service = new AuthService(
            userRepository.Object,
            jwtService.Object,
            NullLogger<AuthService>.Instance
        );
        
        var result = await service.LoginAsync(request);
        
        Assert.Equal("fake-token", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(86400, result.ExpiresIn);
        Assert.Equal(user.UserId, result.User.UserId);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal(user.FirstName, result.User.FirstName);
        Assert.Equal(user.LastName, result.User.LastName);
        Assert.Equal(user.Role, result.User.Role);
        
        jwtService.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ThrowsExceptions()
    {
        var userRepository = new Mock<IUserRepository>();
        var jwtService = new Mock<IJwtService>();
        
        var request = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = "StrongPassword1!"
        };
        
        userRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        
        var service = new AuthService(
            userRepository.Object,
            jwtService.Object,
            NullLogger<AuthService>.Instance
        );
        
        await Assert.ThrowsAsync<LoginFailedException>(() => service.LoginAsync(request));
        
        userRepository.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_PasswordMismatch_ThrowsException()
    {
        var userRepository = new Mock<IUserRepository>();
        var jwtService = new Mock<IJwtService>();
        
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "user@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("StrongPassword1!"),
            FirstName = "John",
            LastName = "Smith",
            Role = Role.Patron,
            MembershipStatus = MembershipStatus.Active,
        };
        
        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = "StrongPassword2!"
        };
        
        userRepository.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        
        var service = new AuthService(
            userRepository.Object,
            jwtService.Object,
            NullLogger<AuthService>.Instance
        );
        
        await Assert.ThrowsAsync<LoginFailedException>(() => service.LoginAsync(request));
        
        userRepository.Verify(x => x.GetByEmailAsync(request.Email), Times.Once);
        
        
    }
}