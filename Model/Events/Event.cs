namespace Model.Events;

public abstract record Event : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
}