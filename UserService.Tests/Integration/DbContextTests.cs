using UserService.Enums;
using UserService.Services;

namespace UserService.Tests.Integration;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserService.Data;
using UserService.Models;
using Xunit;

public sealed class DbContextTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;

    public DbContextTests(ApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task DbContext_can_save_and_read_entity()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = "integration@test.com",
            PasswordHash = "test-hash",
            FirstName = "Integration",
            LastName = "Test",
            PhoneNumber = "555-0100",
            Role = Role.Patron,
            MembershipStatus = MembershipStatus.Active
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var savedUser = await db.Users
            .SingleAsync(x => x.Email == "integration@test.com");

        Assert.Equal(user.UserId, savedUser.UserId);
    }
}