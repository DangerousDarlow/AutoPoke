namespace Model.Events;

public abstract record Event : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}