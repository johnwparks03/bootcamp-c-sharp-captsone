using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUsersService usersService, ILogger<UsersController> logger)
    {
        _usersService = usersService;
        _logger = logger;
    }
    
    [HttpGet("/profile")]
    [Authorize]
    public async Task<ActionResult<ProfileDto>> GetUserProfile(CancellationToken token = default)
    {
        var userId = Guid.Parse(User.FindFirst("userId")?.Value ??
                     User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        _logger.LogInformation($"Getting user profile for userId:  {userId}");
        var response = await _usersService.GetProfileAsync(userId, token);
        return Ok(response);
    }

    [HttpGet("/{userId}/validate")]
    public async Task<ActionResult<ValidatedUserDto>> ValidateUser([FromRoute] string userId, CancellationToken token = default)
    {
        _logger.LogInformation($"Validating user with id: {userId}");
        var parsedUserId = Guid.Parse(userId);
        return Ok(await _usersService.ValidateUserAsync(parsedUserId, token));
    }
    
    
}