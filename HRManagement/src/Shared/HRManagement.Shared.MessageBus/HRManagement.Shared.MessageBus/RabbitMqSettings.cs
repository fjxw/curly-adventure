namespace HRManagement.Shared.MessageBus;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";
    
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "hr_management_events";
    public int RetryCount { get; set; } = 5;
    public int RetryDelayMs { get; set; } = 2000;
}
