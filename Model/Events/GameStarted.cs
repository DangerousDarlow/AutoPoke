namespace Model.Events;

/// <summary>
/// Sent from the engine to all players when a game starts
/// </summary>
public record GameStarted : Event
{
    public Game Game { get; init; } = null!;
}