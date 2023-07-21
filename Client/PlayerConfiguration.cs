namespace Client;

public record PlayerConfiguration
{
    public string Name { get; init; } = null!;
    
    public string Strategy { get; init; } = null!;
}