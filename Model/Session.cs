namespace Model;

public record Session
{
    public Guid SessionId { get; init; } = Guid.NewGuid();
    
    public int Games { get; init; }
}