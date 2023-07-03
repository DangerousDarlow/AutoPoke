namespace Logic;

public record Player
{
    public Guid Id { get; init; }

    public string Name { get; set; } = null!;
}