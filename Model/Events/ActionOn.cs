namespace Model.Events;

public record ActionOn : Event
{
    public Guid Player { get; init; }
}