using System.Collections.Immutable;

namespace Model;

public record Hand
{
    // Must have init setter otherwise the deserialization will fail
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public Guid Id { get; init; } = Guid.NewGuid();

    public ImmutableList<Player> Players { get; init; } = null!;

    public int Sequence { get; init; }

    public int SmallBlind { get; init; }

    public int BigBlind => SmallBlind * 2;
}