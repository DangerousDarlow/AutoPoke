namespace Model.Events;

/// <summary>
/// Triggers the engine to begin a poker session
/// Sent from an additional process (script) to the engine
/// </summary>
public record BeginSession : Event
{
    public int Games { get; init; }
}