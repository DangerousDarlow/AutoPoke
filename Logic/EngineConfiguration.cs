namespace Logic;

public record EngineConfiguration
{
    public int MaxPlayers { get; set; } = 10;

    public int StartingStack { get; set; } = 1000;
}