namespace UserService.Clients;

public class ReservationClient : IReservationClient
{
    
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReservationClient> _logger;

    public ReservationClient(HttpClient httpClient, ILogger<ReservationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ReservationStatistics> GetUserReservationStatisticsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/reservations/statistics/{userId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ReservationStatistics
                {
                    ActiveReservationCount = 0,
                    BorrowingHistoryCount = 0
                };
            }
            
            return await response.Content.ReadFromJsonAsync<ReservationStatistics>(cancellationToken) ?? new ReservationStatistics
            {
                ActiveReservationCount = 0,
                BorrowingHistoryCount = 0
            };
        }catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Reservation Service timed out for {UserId}",
                userId);

            return new ReservationStatistics
            {
                ActiveReservationCount = 0,
                BorrowingHistoryCount = 0
            };
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Reservation Service was unavailable for {UserId}",
                userId);

            return new ReservationStatistics
            {
                ActiveReservationCount = 0,
                BorrowingHistoryCount = 0
            };
        }
    }
}

public class ReservationStatistics
{
    public int ActiveReservationCount { get; set; }
    public int BorrowingHistoryCount { get; set; }
}
