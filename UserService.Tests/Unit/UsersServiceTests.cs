using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserService.Clients;
using UserService.DTOs;
using UserService.Enums;
using UserService.Exceptions;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;

namespace UserService.Tests.Unit;

public class UsersServiceTests
{
    [Fact]
    public async Task GetProfileAsync_ValidUserId_ReturnsProfile()
    {
        var userRepository = new Mock<IUserRepository>();
        var reservationClient = new Mock<IReservationClient>();

        var user = new User
        {
            UserId = new Guid(),
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "Test",
            PhoneNumber = "123-456-7891",
            Role = Role.Patron,
            MembershipStatus = MembershipStatus.Active,
        };

        var profile = new ProfileDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            MembershipStatus = user.MembershipStatus,
            ActiveReservations = 0,
            BorrowingHistory = 0
        };

        userRepository.Setup(x => x.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        reservationClient.Setup(x => x.GetUserReservationStatisticsAsync(user.UserId)).ReturnsAsync(
            new ReservationStatistics
            {
                ActiveReservationCount = 0,
                BorrowingHistoryCount = 0
            });

        var service = new UsersService(
            userRepository.Object,
            reservationClient.Object,
            NullLogger<UsersService>.Instance
        );
        
        var response = await service.GetProfileAsync(user.UserId);
        
        Assert.Equal(profile.UserId, response.UserId);
        Assert.Equal(profile.Email, response.Email);
        Assert.Equal(profile.FirstName, response.FirstName);
        Assert.Equal(profile.LastName, response.LastName);
        Assert.Equal(profile.PhoneNumber, response.PhoneNumber);
        Assert.Equal(profile.Role, response.Role);
        Assert.Equal(profile.MembershipStatus, response.MembershipStatus);
        Assert.Equal(profile.ActiveReservations, response.ActiveReservations);
        Assert.Equal(profile.BorrowingHistory, response.BorrowingHistory);
    }

    [Fact]
    public async Task GetProfileAsync_InvalidUserId_ThrowsNotFoundException()
    {
        var userRepository = new Mock<IUserRepository>();
        var reservationClient = new Mock<IReservationClient>();

        var user = new User
        {
            UserId = new Guid(),
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "Test",
            PhoneNumber = "123-456-7891",
            Role = Role.Patron,
            MembershipStatus = MembershipStatus.Active,
        };
        
        userRepository.Setup(x => x.GetByIdAsync(user.UserId)).ReturnsAsync((User?)null);
        
        var service = new UsersService(
            userRepository.Object,
            reservationClient.Object,
            NullLogger<UsersService>.Instance
        );
        
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetProfileAsync(user.UserId));
        
        userRepository.Verify(x => x.GetByIdAsync(user.UserId), Times.Once);
    }
    
    [Fact]
    public async Task ValidateUserAsync_ValidUserId_ReturnsValidResponse()
    {
        var userRepository = new Mock<IUserRepository>();
        var reservationClient = new Mock<IReservationClient>();

        var user = new User
        {
            UserId = new Guid(),
            Email = "john@test.com",
            FirstName = "John",
            LastName = "Test",
            Role = Role.Patron,
            MembershipStatus = MembershipStatus.Active,
        };

        var validatedUser = new ValidatedUserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            MembershipStatus = user.MembershipStatus,
            ActiveReservations = 2,
        };
        
        userRepository.Setup(x => x.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        reservationClient.Setup(x => x.GetUserReservationStatisticsAsync(user.UserId)).ReturnsAsync(new ReservationStatistics
        {
            ActiveReservationCount = validatedUser.ActiveReservations,
            BorrowingHistoryCount = 0
        });
        
        var service = new UsersService(
            userRepository.Object,
            reservationClient.Object,
            NullLogger<UsersService>.Instance
        );
        
        var response = await service.ValidateUserAsync(validatedUser.UserId);
        
        Assert.Equal(validatedUser.UserId, response.UserId);
        Assert.Equal(validatedUser.Email, response.Email);
        Assert.Equal(validatedUser.FirstName, response.FirstName);
        Assert.Equal(validatedUser.LastName, response.LastName);
        Assert.Equal(validatedUser.Role, response.Role);
        Assert.Equal(validatedUser.MembershipStatus, response.MembershipStatus);
        Assert.Equal(validatedUser.ActiveReservations, response.ActiveReservations);
    }
    
    [Fact]
    public async Task ValidateUserAsync_InvalidUserId_ThrowsNotFoundException()
    {
        var userRepository = new Mock<IUserRepository>();
        var reservationClient = new Mock<IReservationClient>();

        var user = new User
        {
            UserId = new Guid(),
        };
        
        userRepository.Setup(x => x.GetByIdAsync(user.UserId)).ReturnsAsync((User?)null);
        
        var service = new UsersService(
            userRepository.Object,
            reservationClient.Object,
            NullLogger<UsersService>.Instance
        );
        
        await Assert.ThrowsAsync<NotFoundException>(() => service.ValidateUserAsync(user.UserId));
        
    }
    
    [Fact]
    public async Task ValidateUserAsync_SuspendedUser_ThrowsSuspendedException()
    {
        var userRepository = new Mock<IUserRepository>();
        var reservationClient = new Mock<IReservationClient>();
        
        var user = new User
        {
            UserId = new Guid(),
            MembershipStatus = MembershipStatus.Suspended,
        };
        
        userRepository.Setup(x => x.GetByIdAsync(user.UserId)).ReturnsAsync(user);
        
        var service = new UsersService(
            userRepository.Object,
            reservationClient.Object,
            NullLogger<UsersService>.Instance
        );
        
        await Assert.ThrowsAsync<SuspendedUserException>(() => service.ValidateUserAsync(user.UserId));
    }
}