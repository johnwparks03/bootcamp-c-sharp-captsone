using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserService.Controllers;
using UserService.DTOs;
using UserService.Enums;
using UserService.Services;

namespace UserService.Tests.Unit;

public class UsersControllerTests
{
    
    [Fact]
    public async Task Profile_AuthenticatedUser_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var usersService = new Mock<IUsersService>();

        var expectedProfile = new ProfileDto
        {
            UserId = userId,
            Email = "john@example.com"
        };

        usersService
            .Setup(x => x.GetProfileAsync(userId))
            .ReturnsAsync(expectedProfile);

        var controller = new UsersController(
            usersService.Object,
            NullLogger<UsersController>.Instance);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            },
            authenticationType: "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var result = await controller.GetUserProfile();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expectedProfile, okResult.Value);

        usersService.Verify(
            x => x.GetProfileAsync(userId),
            Times.Once);
    }
    
    [Fact]
    public async Task Validate_ValidUserId_ReturnsOk()
    {
        var userService = new Mock<IUsersService>();
        
        Guid userId = Guid.NewGuid();
        string strUserId = userId.ToString();

        var expected = new ValidatedUserDto
        {
            UserId = userId,
            Email = "john@test.com",
            FirstName = "John",
            LastName = "Doe",
            ActiveReservations = 2,
            MembershipStatus = MembershipStatus.Active,
            Role = Role.Patron
        };
        
        userService.Setup(x => x.ValidateUserAsync(userId)).ReturnsAsync(expected);

        var controller = new UsersController(
            userService.Object,
            NullLogger<UsersController>.Instance
        );
        
        var actionResult = await controller.ValidateUser(strUserId);
        
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(expected, okResult.Value);
    }
    
}
