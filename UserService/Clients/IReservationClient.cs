namespace UserService.Clients;

public interface IReservationClient
{
    Task<ReservationStatistics> GetUserReservationStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
}

