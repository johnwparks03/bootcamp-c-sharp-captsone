using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Exceptions;
using UserService.Services;

namespace UserService.Controllers;

/// <summary>
/// Controller responsible for user authentication and authorization operations
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("Processing register request");
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid register request");
            return BadRequest(CreateValidationErrorResponse());
        }
        
        var response = await _authService.RegisterAsync(request);
        return Created($"{response.UserId}", response);
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Processing login request");
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid login request");
            return BadRequest(CreateValidationErrorResponse());
        }
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
    
    private object CreateValidationErrorResponse()
    {
        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new
        {
            message = "Validation failed",
            errors = errors
        };
    }
}   