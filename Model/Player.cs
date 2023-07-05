namespace Model;

public record Player
{
    public Guid Id { get; init; }

    public string Name { get; set; } = null!;
}