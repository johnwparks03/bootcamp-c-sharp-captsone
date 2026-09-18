using UserService.DTOs;

namespace UserService.Services;

public interface IUsersService
{
    Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken=default);
    Task<ValidatedUserDto> ValidateUserAsync(Guid userId, CancellationToken cancellationToken=default);
}