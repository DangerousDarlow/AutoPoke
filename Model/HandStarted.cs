namespace Model;

public record HandStarted : Event
{
    public Hand Hand { get; init; } = null!;
}