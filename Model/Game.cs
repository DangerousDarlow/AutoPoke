namespace Model;

public record Game
{
    // Must have init setter otherwise the deserialization will fail
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public Guid Id { get; init; } = Guid.NewGuid();
}