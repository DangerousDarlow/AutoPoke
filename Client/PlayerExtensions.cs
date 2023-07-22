namespace Client;

public static class PlayerExtensions
{
    public static Model.Player MeInModel(this IPlayer player)
    {
        if (player.Hand == null) throw new InvalidOperationException("Hand is null");
        return player.Hand.Players.Single(p => p.Id == player.Id) ?? throw new InvalidOperationException("Player not found in hand");
    }
}