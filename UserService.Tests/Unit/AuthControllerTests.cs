using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserService.Controllers;
using UserService.DTOs;
using UserService.Enums;
using UserService.Services;

namespace UserService.Tests.Unit;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var authService = new Mock<IAuthService>();

        var request = new RegisterRequestDto
        {
            Email = "john@example.com",
            Password = "StrongPassword1!",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };

        var expectedResponse = new RegisterResponseDto
        {
            UserId = Guid.NewGuid(),
            Email = request.Email
        };

        authService
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(expectedResponse
        );

        var controller = new AuthController(
            authService.Object,
            NullLogger<AuthController>.Instance
        );

        var actionResult = await controller.Register(request);

        var createdResult = Assert.IsType<CreatedResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Same(expectedResponse, createdResult.Value);

        authService.Verify(
            x => x.RegisterAsync(request),
            Times.Once
        );
    }
    
    [Fact]
    public async Task Register_InvalidModelState_ReturnsBadRequest()
    {
        var authService = new Mock<IAuthService>();

        var controller = new AuthController(
            authService.Object,
            NullLogger<AuthController>.Instance
        );

        controller.ModelState.AddModelError(
            nameof(RegisterRequestDto.Email),
            "Email is required"
        );

        var actionResult = await controller.Register(
            new RegisterRequestDto()
        );

        var badRequest = Assert.IsType<BadRequestObjectResult>(
            actionResult.Result
        );

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            badRequest.StatusCode
        );

        authService.Verify(
            x => x.RegisterAsync(It.IsAny<RegisterRequestDto>()),
            Times.Never
        );
    }
    
    [Fact]
    public async Task Login_ValidRequest_ReturnsOk()
    {
        // Arrange
        var authService = new Mock<IAuthService>();

        var request = new LoginRequestDto
        {
            Email = "john@example.com",
            Password = "StrongPassword1!",
        };

        var expectedResponse = new LoginResponseDto
        {
            AccessToken = "fake-token",
            TokenType = "Bearer",
            ExpiresIn = 86400,
            User= new LoginUserDto{
                UserId = new Guid(),
                Email = "john@test.com",
                FirstName = "John",
                LastName = "Doe",
                Role = Role.Patron
            }
        };

        authService
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(expectedResponse);

        var controller = new AuthController(
            authService.Object,
            NullLogger<AuthController>.Instance
        );

        var actionResult = await controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expectedResponse, okResult.Value);

        authService.Verify(
            x => x.LoginAsync(request),
            Times.Once
        );
    }
    
    [Fact]
    public async Task Login_MissingEmail_ReturnsBadRequest()
    {
        var authService = new Mock<IAuthService>();

        var controller = new AuthController(
            authService.Object,
            NullLogger<AuthController>.Instance
        );

        controller.ModelState.AddModelError(
            nameof(RegisterRequestDto.Email),
            "Email is required"
        );

        var actionResult = await controller.Login(
            new LoginRequestDto()
        );

        var badRequest = Assert.IsType<BadRequestObjectResult>(
            actionResult.Result
        );

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            badRequest.StatusCode
        );

        authService.Verify(
            x => x.LoginAsync(It.IsAny<LoginRequestDto>()),
            Times.Never
        );
    }
}