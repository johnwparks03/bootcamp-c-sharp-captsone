namespace UserService.Configuration;

public class ExternalServiceSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string HealthEndpoint { get; set; } = "/health";
    public int TimeoutSeconds { get; set; } = 30;
}