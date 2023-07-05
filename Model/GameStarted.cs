namespace Model;

public record GameStarted : Event
{
    public Game Game { get; init; } = null!;
}