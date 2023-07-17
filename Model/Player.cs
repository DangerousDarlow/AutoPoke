namespace Model;

public record Player
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public int Stack { get; init; }
}

public static class PlayerExtensions
{
    public static Player WithStack(this Player player, int stack) => new()
    {
        Id = player.Id,
        Name = player.Name,
        Stack = stack
    };
}