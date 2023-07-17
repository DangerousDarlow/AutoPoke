using System.Collections.Immutable;

namespace Model;

public record GameStarted : Event
{
    public Game Game { get; init; } = null!;

    public ImmutableList<Player> Players { get; init; } = null!;
}