namespace Events;

/// <summary>
/// Published by a client / player when requesting to join a session
/// </summary>
public class JoinRequest : IEvent
{
    /// <summary>
    /// Uniquely identifies the player
    /// </summary>
    public Guid PlayerId { get; init; }

    /// <summary>
    /// Human readable name for the player. Used only to make output, logs and metrics human readable.
    /// </summary>
    public string? PlayerName { get; init; }
}