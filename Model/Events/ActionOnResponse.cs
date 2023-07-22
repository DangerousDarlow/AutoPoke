namespace Model.Events;

/// <summary>
/// Player response to an ActionOn event
/// Sent from a player to the engine
/// </summary>
public record ActionOnResponse : Event
{
    /// <summary>
    /// Id of the ActionOn event this is a response to
    /// </summary>
    public Guid ResponseTo { get; init; }

    public Action Action { get; init; } = null!;
}