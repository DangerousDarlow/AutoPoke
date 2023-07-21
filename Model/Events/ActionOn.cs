namespace Model.Events;

/// <summary>
/// Triggers a player to act
/// Sent from the engine to a single player
/// </summary>
public record ActionOn : Event;