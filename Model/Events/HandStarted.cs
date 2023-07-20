namespace Model.Events;

public record HandStarted : Event
{
    public Hand Hand { get; init; } = null!;
}