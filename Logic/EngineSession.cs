using Model;

namespace Logic;

public record EngineSession
{
    public Session Session { get; init; } = null!;

    public int GamesPlayed { get; init; } = 0;
}