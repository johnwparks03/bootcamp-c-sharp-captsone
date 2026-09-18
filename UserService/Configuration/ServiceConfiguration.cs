namespace UserService.Configuration;

public class ServiceConfiguration
{
    public string ServiceName { get; set; } = string.Empty;
    public int Port {get; set;}
    public string HealthCheckPath { get; set; } = "/health";
}