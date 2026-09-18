using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Castle.Core.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using UserService.Enums;
using UserService.Models;
using UserService.Services;

namespace UserService.Tests.Unit;

public class JwtServiceTests
{
    private const string SECRET = "test-secret-key-needs-to-be-longer";
    private const string ISSUER = "TestIssuer";
    private const string AUDIENCE = "TestAudience";

    private static JwtService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = SECRET,
                ["Jwt:Issuer"] = ISSUER,
                ["Jwt:Audience"] = AUDIENCE,
            })
            .Build();
        return new JwtService(config);
    }

    [Fact]
    public async Task GenerateToken_ContainsExpectedClaims()
    {
        var service = CreateService();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "John@test.com",
            Role = Role.Patron
        };
        
        var token = service.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        
        Assert.Equal(ISSUER, jwt.Issuer);
        Assert.Contains(AUDIENCE, jwt.Audiences);
        
        Assert.Equal(
            user.UserId.ToString(),
            jwt.Claims.First(x =>
                x.Type == ClaimTypes.NameIdentifier).Value);

        Assert.Equal(
            user.UserId.ToString(),
            jwt.Claims.First(x =>
                x.Type == "UserId").Value);

        Assert.Equal(
            user.Email,
            jwt.Claims.First(x =>
                x.Type == ClaimTypes.Email).Value);

        Assert.Equal(
            user.Role.ToString(),
            jwt.Claims.First(x =>
                x.Type == ClaimTypes.Role).Value);
    }
    
    [Fact]
    public void ValidateToken_ReturnsTrueForValidToken()
    {
        var service = CreateService();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "john@test.com",
            Role = Role.Patron
        };
        var token = service.GenerateToken(user);

        Assert.True(service.ValidateToken(token));
    }

    [Fact]
    public void ValidateToken_ReturnsFalseForInvalidToken()
    {
        var service = CreateService();

        Assert.False(service.ValidateToken("not-a-valid-jwt"));
    }
    
    [Fact]
    public void GetEmailFromToken_ReturnsEmail()
    {
        var service = CreateService();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "john@test.com",
            Role = Role.Patron
        };

        var token = service.GenerateToken(user);

        Assert.Equal(user.Email, service.GetEmailFromToken(token));
    }

    [Fact]
    public void GetEmailFromToken_ReturnsNullForInvalidoken()
    {
        var service = CreateService();

        Assert.Null(service.GetEmailFromToken("not-a-valid-jwt"));
    }
}