namespace Model.Events;

public record ActionOnResponse : Event
{
    public Guid ActionOn { get; init; }

    public Action Action { get; init; }
}