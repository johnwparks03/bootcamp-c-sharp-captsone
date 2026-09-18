using UserService.Clients;
using UserService.DTOs;
using UserService.Enums;
using UserService.Exceptions;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services;

public class UsersService : IUsersService
{
    private readonly IUserRepository _userRepository;
    private readonly IReservationClient _reservationClient;
    private readonly ILogger<UsersService> _logger;

    public UsersService(IUserRepository userRepository, IReservationClient reservationClient, ILogger<UsersService> logger)
    {
        _userRepository = userRepository;
        _reservationClient = reservationClient;
        _logger = logger;
    }


    public async Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken token = default)
    {
        
        var user =  await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogError($"User with id: {userId} not found");
            throw new NotFoundException();
        }
        
        var reservationStatistics = await _reservationClient.GetUserReservationStatisticsAsync(userId, token);
        
        _logger.LogInformation($"Profile retrieved successful for : {user.Email}");
        return new ProfileDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            MembershipStatus = user.MembershipStatus,
            ActiveReservations = reservationStatistics?.ActiveReservationCount ?? 0,
            BorrowingHistory = reservationStatistics?.BorrowingHistoryCount ?? 0
        };
    }

    public async Task<ValidatedUserDto> ValidateUserAsync(Guid userId, CancellationToken token = default)
    {
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogError($"User with id: {userId} not found");
            throw new NotFoundException();
        }

        if (user.MembershipStatus == MembershipStatus.Suspended)
        {
            _logger.LogError($"User with id: {userId} is suspended");
            throw new SuspendedUserException();
        }
        
        var reservationStatistics = await _reservationClient.GetUserReservationStatisticsAsync(userId, token);
        
        _logger.LogInformation("Validated user retrieved successfully");
        return new ValidatedUserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            MembershipStatus = user.MembershipStatus,
            ActiveReservations = reservationStatistics?.ActiveReservationCount ?? 0,
        };
    }
}