namespace Model.Events;

/// <summary>
/// Triggers the server to begin a poker session
/// A session consists of multiple games
/// </summary>
public record BeginSession : Event
{
    public int Games { get; init; }
}