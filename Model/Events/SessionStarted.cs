namespace Model.Events;

/// <summary>
/// Sent from the engine to all players when a session starts
/// </summary>
public record SessionStarted : Event
{
    public Session Session { get; init; } = null!;
}