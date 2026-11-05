namespace SmartPalm.AgronomyService.Infrastructure.Configuration;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public bool Enabled { get; set; }
    public string Uri { get; set; } = "amqp://guest:guest@localhost:5672/";
    public string Exchange { get; set; } = "smartpalm.events";
    public string Queue { get; set; } = "smartpalm.agronomy";
}
