namespace Model;

public record Session
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public int Games { get; init; }
}