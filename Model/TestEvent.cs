namespace Model;

public record TestEvent : Event
{
    public string? Value { get; init; }
}