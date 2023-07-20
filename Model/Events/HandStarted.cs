namespace Model.Events;

/// <summary>
/// Sent from the engine to all players when a hand starts
/// </summary>
public record HandStarted : Event
{
    public Hand Hand { get; init; } = null!;
}