namespace Model.Events;

public record GameStarted : Event
{
    public Game Game { get; init; } = null!;
}