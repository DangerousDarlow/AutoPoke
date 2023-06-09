﻿namespace Model;

/// <summary>
/// Sent by a client to the server when requesting to join a session
/// </summary>
public record JoinRequest : Event
{
    public Guid PlayerId { get; init; }

    /// <summary>
    /// Human readable name for the player. Used only to make output human readable.
    /// </summary>
    public string PlayerName { get; init; } = null!;
}