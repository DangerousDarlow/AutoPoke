namespace ZeroMq;

public record ZeroMqConfiguration
{
    public string? DealerAddress { get; set; }
    public string? PublisherAddress { get; set; }
    public string? RouterAddress { get; set; }
    public string? SubscriberAddress { get; set; }
}