namespace Model.Events;

/// <summary>
/// Player response to an ActionOn event
/// Sent from a player to the engine
/// </summary>
public record ActionOnResponse : Event
{
    public Guid ActionOn { get; init; }

    public Action Action { get; init; }
}