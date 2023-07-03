namespace Model;

public record TestEvent : IEvent
{
    public string? Value { get; init; }
}