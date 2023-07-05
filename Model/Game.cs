using System.Collections.Immutable;

namespace Model;

public record Game
{
    // Must have init setter otherwise the deserialization will fail
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public Guid Id { get; init; } = Guid.NewGuid();

    public int Sequence { get; init; }

    public IImmutableList<Player> Players { get; init; } = null!;

    public int StartingStack { get; init; }
}