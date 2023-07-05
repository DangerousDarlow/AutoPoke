namespace Model;

public record TestEvent : Event
{
    // All event properties must have init setters otherwise the deserialization will fail
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public Guid Id { get; init; } = Guid.NewGuid();

    public string? Value { get; init; }
}