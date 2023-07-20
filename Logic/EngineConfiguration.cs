namespace Logic;

public record EngineConfiguration
{
    public int MaximumNumberOfPlayers { get; set; } = 10;

    /// <summary>
    /// Stack for each player at the start of each game
    /// </summary>
    public int InitialStack { get; set; } = 1000;

    /// <summary>
    /// Small blind at the start of each game
    /// </summary>
    public int InitialSmallBlind { get; set; } = 1;

    /// <summary>
    /// Number of hands played at each blind level
    /// </summary>
    public int HandsPerBlindLevel { get; set; } = 40;
}