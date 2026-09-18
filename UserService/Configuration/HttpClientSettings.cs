namespace UserService.Configuration;

public class HttpClientSettings
{
    public int DefaultTimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}