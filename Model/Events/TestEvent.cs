namespace Model.Events;

public record TestEvent : Event
{
    public string? Value { get; init; }
}