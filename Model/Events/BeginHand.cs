namespace Model.Events;

/// <summary>
/// Triggers the engine to begin a poker hand
/// Sent from the engine to itself. The engine will only action this event if it originated from itself.
/// </summary>
public record BeginHand : Event;