namespace Model;

/// <summary>
/// Sent by the server to all clients when a session starts
/// </summary>
public record SessionStarted : Event
{
    public Session Session { get; init; } = new();
}