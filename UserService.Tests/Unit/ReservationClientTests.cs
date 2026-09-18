using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging.Abstractions;
using UserService.Clients;

namespace UserService.Tests.Unit;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage response;

    public HttpRequestMessage? Request { get; private set; }

    public FakeHttpMessageHandler(HttpResponseMessage response)
    {
        this.response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Request = request;
        return Task.FromResult(response);
    }
}

public sealed class ThrowingHttpMessageHandler : HttpMessageHandler
{
    private Exception exception;

    public ThrowingHttpMessageHandler(Exception exception)
    {
        this.exception = exception;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        throw exception;
    }
}

public class ReservationClientTests
{
    [Fact]
    public async Task GetUserReservationStatisticsAsync_FailedResponseStatusCode_ReturnsZero()
    {
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.NotFound)
        );

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
            
        var client = new ReservationClient(httpClient, NullLogger<ReservationClient>.Instance);

        var result = await client.GetUserReservationStatisticsAsync(Guid.NewGuid());

        Assert.Equal(0, result.ActiveReservationCount);
        Assert.Equal(0, result.BorrowingHistoryCount);
        Assert.Equal(HttpMethod.Get, handler.Request!.Method);
    }
    
    [Fact]
    public async Task GetUserReservationStatisticsAsync_SuccessStatusCode_ReturnsReservationStatistics()
    {
        var expected = new ReservationStatistics
        {
            ActiveReservationCount = 1,
            BorrowingHistoryCount = 1,
        };
        
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expected)
            }
        );

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
            
        var client = new ReservationClient(httpClient, NullLogger<ReservationClient>.Instance);

        var result = await client.GetUserReservationStatisticsAsync(Guid.NewGuid());

        Assert.Equal(1, result.ActiveReservationCount);
        Assert.Equal(1, result.BorrowingHistoryCount);
        Assert.Equal(HttpMethod.Get, handler.Request!.Method);
    }
    
    [Fact]
    public async Task GetUserReservationStatisticsAsync_Timeout_ReturnsZero()
    {
        var handler = new ThrowingHttpMessageHandler(
            new TaskCanceledException()
        );

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
            
        var client = new ReservationClient(httpClient, NullLogger<ReservationClient>.Instance);

        var result = await client.GetUserReservationStatisticsAsync(Guid.NewGuid());

        Assert.Equal(0, result.ActiveReservationCount);
        Assert.Equal(0, result.BorrowingHistoryCount);
    }
    
    [Fact]
    public async Task GetUserReservationStatisticsAsync_Unavailable_ReturnsZero()
    {
        var handler = new ThrowingHttpMessageHandler(
            new HttpRequestException()
        );

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
            
        var client = new ReservationClient(httpClient, NullLogger<ReservationClient>.Instance);

        var result = await client.GetUserReservationStatisticsAsync(Guid.NewGuid());

        Assert.Equal(0, result.ActiveReservationCount);
        Assert.Equal(0, result.BorrowingHistoryCount);
    }
}