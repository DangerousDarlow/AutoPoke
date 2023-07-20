namespace Model.Events;

/// <summary>
/// Sent from a player to the engine to request to join the game
/// </summary>
public record JoinRequest : Event
{
    public Guid PlayerId { get; init; }

    /// <summary>
    /// Human readable name for the player. Used only to make output human readable.
    /// </summary>
    public string PlayerName { get; init; } = null!;
}